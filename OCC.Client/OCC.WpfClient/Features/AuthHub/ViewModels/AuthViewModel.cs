using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;
using OCC.WpfClient.Services.Infrastructure;
using Microsoft.Extensions.Logging;
using OCC.WpfClient.Features.AuthHub.Models;

namespace OCC.WpfClient.Features.AuthHub.ViewModels
{
    public partial class AuthViewModel : ViewModelBase
    {
        private readonly ILogger<AuthViewModel> _logger;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly LocalSettingsService _localSettings;
        private readonly ConnectionSettings _connectionSettings;

        [ObservableProperty]
        private LoginRequest _loginModel = new();

        [ObservableProperty]
        private RegisterRequest _registerModel = new();

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private bool _isDevUser;

        public ConnectionSettings.AppEnvironment SelectedEnvironment
        {
            get => _connectionSettings.SelectedEnvironment;
            set
            {
                if (_connectionSettings.SelectedEnvironment != value)
                {
                    _connectionSettings.SelectedEnvironment = value;
                    OnPropertyChanged();
                }
            }
        }

        public Array Environments => Enum.GetValues(typeof(ConnectionSettings.AppEnvironment));

        public AuthViewModel(ILogger<AuthViewModel> logger,
                            IAuthService authService, 
                            INavigationService navigationService,
                            LocalSettingsService localSettings,
                            ConnectionSettings connectionSettings)
        {
            _logger = logger;
            _authService = authService;
            _navigationService = navigationService;
            _localSettings = localSettings;
            _connectionSettings = connectionSettings;

            // Load persistence settings
            LoginModel.Email = _localSettings.Settings.LastEmail;
            LoginModel.RememberMe = _localSettings.Settings.RememberMe;

#if DEBUG
            IsDevUser = true;
#else
            IsDevUser = false;
#endif

            _connectionSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ConnectionSettings.SelectedEnvironment))
                {
                    OnPropertyChanged(nameof(SelectedEnvironment));
                }
            };
            Title = "Authentication";
        }


        #region Commands

        [RelayCommand]
        private async Task LoginAsync(object? parameter)
        {
            _logger.LogInformation("Login command executed.");
            if (parameter is not System.Windows.Controls.PasswordBox passwordBox)
            {
                _logger.LogWarning("Login command parameter is not a PasswordBox.");
                return;
            }

            base.ErrorMessage = null;
            LoginModel.Password = passwordBox.Password;
            LoginModel.Validate();

            if (LoginModel.HasErrors)
            {
                _logger.LogWarning("Login validation failed. Errors: {Errors}", string.Join(", ", LoginModel.GetErrors().Select(e => e.ErrorMessage)));
                ErrorMessage = "Please correct the errors before logging in.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                var (success, error) = await _authService.LoginAsync(LoginModel.Email, LoginModel.Password);
                
                if (success)
                {
                    // Save persistence settings
                    _localSettings.Settings.RememberMe = LoginModel.RememberMe;
                    _localSettings.Settings.LastEmail = LoginModel.RememberMe ? LoginModel.Email : string.Empty;
                    _localSettings.Save();

                    _navigationService.NavigateTo<Main.ViewModels.MainViewModel>();
                }
                else
                {
                    ErrorMessage = error ?? "Login failed.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RegisterAsync(object? parameter)
        {
            if (parameter is not System.Windows.Controls.PasswordBox passwordBox)
                return;

            RegisterModel.Password = passwordBox.Password;
            // Note: ConfirmPassword would need its own passwordbox for full validation, 
            // for now we match it to simplify the demo or assume it's set elsewhere.
            RegisterModel.ConfirmPassword = passwordBox.Password; 
            
            RegisterModel.Validate();

            if (RegisterModel.HasErrors)
            {
                ErrorMessage = "Please fill in all required fields correctly.";
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;
                
                var user = new User
                {
                    Email = RegisterModel.Email,
                    FirstName = RegisterModel.FirstName,
                    LastName = RegisterModel.LastName,
                    Password = RegisterModel.Password
                };

                var success = await _authService.RegisterAsync(user);
                
                if (success)
                {
                    ErrorMessage = "Registration successful! You can now login.";
                    passwordBox.Clear();
                }
                else
                {
                    ErrorMessage = "Registration failed.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}
