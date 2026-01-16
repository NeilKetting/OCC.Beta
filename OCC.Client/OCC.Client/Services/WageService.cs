using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class WageService : IWageService
    {
        private readonly HttpClient _client;
        private readonly IAuthService _authService;

        public WageService(IAuthService authService)
        {
            _authService = authService;
            var baseUrl = OCC.Client.Services.Infrastructure.ConnectionSettings.Instance.ApiBaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        private void AddAuthHeader()
        {
             var token = _authService.AuthToken;
             if (!string.IsNullOrEmpty(token))
             {
                 _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
             }
        }

        public async Task<IEnumerable<WageRun>> GetWageRunsAsync()
        {
            AddAuthHeader();
            return await _client.GetFromJsonAsync<IEnumerable<WageRun>>("api/WageRuns") ?? new List<WageRun>();
        }

        public async Task<WageRun?> GetWageRunByIdAsync(Guid id)
        {
            AddAuthHeader();
            try {
                return await _client.GetFromJsonAsync<WageRun>($"api/WageRuns/{id}");
            } catch { return null; }
        }

        public async Task<WageRun> GenerateDraftRunAsync(DateTime startDate, DateTime endDate, string? notes = null)
        {
            AddAuthHeader();
            var request = new WageRun { StartDate = startDate, EndDate = endDate, Notes = notes };
            var response = await _client.PostAsJsonAsync("api/WageRuns/draft", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WageRun>() ?? throw new Exception("Failed to deserialize response");
        }

        public async Task FinalizeRunAsync(Guid id)
        {
            AddAuthHeader();
            var response = await _client.PostAsync($"api/WageRuns/finalize/{id}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteRunAsync(Guid id)
        {
             AddAuthHeader();
             var response = await _client.DeleteAsync($"api/WageRuns/{id}");
             response.EnsureSuccessStatusCode();
        }
    }
}
