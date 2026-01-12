using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Infrastructure;
using OCC.Client.ViewModels.Core; // Added for ViewModelBase
using System.Collections.ObjectModel;
using System;

namespace OCC.Client.ViewModels.Settings
{
    public partial class UserPreferencesViewModel : ViewModelBase
    {
        private readonly UserActivityService _userActivityService;

        public event EventHandler? CloseRequested;

        [ObservableProperty]
        private int _selectedTimeout;

        public ObservableCollection<int> TimeoutOptions { get; } = new() { 5, 10, 15, 30, 60 };

        public UserPreferencesViewModel(UserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
            
            // Load current timeout
            SelectedTimeout = (int)_userActivityService.LogoutThresholdMinutes;
            if (!TimeoutOptions.Contains(SelectedTimeout))
            {
                 TimeoutOptions.Add(SelectedTimeout);
            }
        }

        [RelayCommand]
        private void Save()
        {
            _userActivityService.UpdateTimeout(SelectedTimeout);
            // Show toast or confirmation if needed? 
            // For now, just close or stay? Typically preferences auto-save or save & close.
            // Let's assume this is a "page" navigation style based on ShellViewModel.
        }

        [RelayCommand]
        private void Back()
        {
             CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
