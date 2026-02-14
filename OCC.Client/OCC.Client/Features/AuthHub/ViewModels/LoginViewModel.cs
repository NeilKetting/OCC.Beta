using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Client.Features.HomeHub.ViewModels;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Core; // Added
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.Services.Infrastructure;


namespace OCC.Client.Features.AuthHub.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly LocalSettingsService _localSettings;
        private readonly ConnectionSettings _connectionSettings;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string? _errorMessage;

        #endregion

        #region Constructors
        public LoginViewModel()
        {
            // Parameterless constructor for design-time support
            _authService = null!;
            _localSettings = null!;
            _connectionSettings = null!;
            _serviceProvider = null!;
        }

        public LoginViewModel(IAuthService authService, LocalSettingsService localSettings, ConnectionSettings connectionSettings, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _localSettings = localSettings;
            _connectionSettings = connectionSettings;
            _serviceProvider = serviceProvider;

            // Load saved email
            Email = _localSettings.Settings.LastEmail;
            
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
        }

        [ObservableProperty]
        private bool _isDevUser;

        // [ObservableProperty]
        // private bool _useLocalDb; // Removed in favor of Enum

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

        public List<ConnectionSettings.AppEnvironment> Environments { get; } = Enum.GetValues<ConnectionSettings.AppEnvironment>().ToList();

        #endregion

        #region Commands

        [RelayCommand]
        public virtual async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return;
            }

            var (success, errorMessage) = await _authService.LoginAsync(Email, Password);
            if (!success)
            {
                ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Invalid email or password." : errorMessage;
            }
            else
            {
                // Save email on successful login
                _localSettings.Settings.LastEmail = Email;
                _localSettings.Save();

                ErrorMessage = null;
                var shellViewModel = _serviceProvider.GetRequiredService<ShellViewModel>();
                WeakReferenceMessenger.Default.Send(new NavigationMessage(shellViewModel));
            }
        }

        #endregion
    }
}



