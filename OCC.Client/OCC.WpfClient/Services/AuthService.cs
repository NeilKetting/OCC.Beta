using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OCC.Shared.DTOs;
using OCC.Shared.Models;
using OCC.WpfClient.Services.Interfaces;
using OCC.WpfClient.Services.Infrastructure;

namespace OCC.WpfClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly ConnectionSettings _connectionSettings;
        private readonly HttpClient _httpClient;
        private readonly ILocalEncryptionService _encryptionService;
        
        private User? _currentUser;
        private string? _authToken;

        public User? CurrentUser => _currentUser;
        public string? CurrentToken => _authToken;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService(ILogger<AuthService> logger, 
                           ConnectionSettings connectionSettings,
                           IHttpClientFactory httpClientFactory,
                           ILocalEncryptionService encryptionService)
        {
            _logger = logger;
            _connectionSettings = connectionSettings;
            _httpClient = httpClientFactory.CreateClient();
            _encryptionService = encryptionService;
        }

        private string GetFullUrl(string path)
        {
            var baseUrl = _connectionSettings.ApiBaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            return $"{baseUrl}{path}";
        }

        public async Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
        {
            var url = GetFullUrl("api/Auth/login");
            _logger.LogInformation("Login attempt started for {Email}. Environment: {Environment}, API URL: {ApiUrl}", 
                email, _connectionSettings.SelectedEnvironment, url);

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, new LoginRequest { Email = email, Password = password });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        _currentUser = result.User;
                        _authToken = result.Token;
                        
                        // Add token to default headers for future requests
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                        
                        // Initialize E2EE RSA Keys
                        _encryptionService.InitializeOrLoadKeys(_currentUser.Id);
                        
                        // If the server doesn't have our public key yet, upload it
                        if (string.IsNullOrEmpty(_currentUser.PublicKey))
                        {
                            _currentUser.PublicKey = _encryptionService.GetPublicKey();
                            var updateUrl = GetFullUrl($"api/users/{_currentUser.Id}");
                            var updateResponse = await _httpClient.PutAsJsonAsync(updateUrl, _currentUser);
                            if (!updateResponse.IsSuccessStatusCode)
                            {
                                _logger.LogWarning("Failed to upload Public RSA Key for {Email}", email);
                            }
                        }

                        _logger.LogInformation("Login successful for {Email}. User: {FirstName} {LastName}", email, _currentUser.FirstName, _currentUser.LastName);
                        return (true, string.Empty);
                    }
                }
                
                // Read error message from API
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = errorContent.Trim('"');
                
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Login failed with status: {response.StatusCode}";
                }

                _logger.LogWarning("Login failed for {Email}. Status: {Status}, Message: {Message}", email, response.StatusCode, errorMessage);
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for {Email} at {Url}", email, url);
                return (false, $"Connection error: {ex.Message}");
            }
        }

        public async Task<bool> RegisterAsync(User user)
        {
            var url = GetFullUrl("api/auth/register");
            _logger.LogInformation("Registration attempt for {Email} at {Url}", user.Email, url);

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, user);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Registration failed for {Email}. Status: {Status}, Error: {Error}", user.Email, response.StatusCode, error);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during registration for {Email}", user.Email);
                return false;
            }
        }

        public Task LogoutAsync()
        {
            _logger.LogInformation("User {Email} logging out.", _currentUser?.Email);
            _currentUser = null;
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return Task.CompletedTask;
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public User User { get; set; } = new();
        }
    }
}
