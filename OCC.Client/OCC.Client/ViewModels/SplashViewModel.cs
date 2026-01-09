using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using OCC.Client.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Velopack;

namespace OCC.Client.ViewModels
{
    public partial class SplashViewModel : ViewModelBase
    {
        private readonly IUpdateService _updateService;
        private readonly Action _onCompleted;

        [ObservableProperty]
        private string _statusText = "Checking for updates...";

        [ObservableProperty]
        private bool _isChecking = true;

        [ObservableProperty]
        private int _progress;

        [ObservableProperty]
        private string _currentVersion;

        public SplashViewModel(IUpdateService updateService, Action onCompleted)
        {
            _updateService = updateService;
            _onCompleted = onCompleted;
            _currentVersion = $"v{updateService.CurrentVersion}";

            _ = CheckUpdates();
        }

        private async Task CheckUpdates()
        {
            try
            {
                // Artificial delay for UX (prevent flicker if too fast)
                await Task.Delay(1500);

                var updateInfo = await _updateService.CheckForUpdatesAsync();
                
                if (updateInfo != null)
                {
                    StatusText = "Update found! Downloading...";
                    IsChecking = false; // Switch to determinate progress if supported
                    
                    await _updateService.DownloadUpdatesAsync(updateInfo, (p) => 
                    {
                        Progress = p;
                    });

                    StatusText = "Installing update...";
                    _updateService.ApplyUpdatesAndExit(updateInfo);
                    // App exits here.
                }
                else
                {
                    StatusText = "Keep building... Log in"; // Easter egg or standard text
                    await Task.Delay(500); 
                    _onCompleted?.Invoke();
                }
            }
            catch (Exception)
            {
                // On error, just proceed to login
                StatusText = "Continuing...";
                await Task.Delay(1000);
                _onCompleted?.Invoke();
            }
        }
    }
}
