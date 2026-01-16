using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Infrastructure;
using OCC.Client.ViewModels.Core;
using System;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Developer
{
    public partial class DeveloperViewModel : ViewModelBase
    {
        private readonly ILogger<DeveloperViewModel> _logger;
        private readonly SignalRNotificationService _signalRService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private string _broadcastMessage = string.Empty;



        public DeveloperViewModel(
            ILogger<DeveloperViewModel> logger,
            SignalRNotificationService signalRService,
            IDialogService dialogService)
        {
            _logger = logger;
            _signalRService = signalRService;
            _dialogService = dialogService;
        }

        public DeveloperViewModel()
        {
            _logger = null!;
            _signalRService = null!;
            _dialogService = null!;
        }

        [RelayCommand]
        public async Task SendBroadcast()
        {
            if (string.IsNullOrWhiteSpace(BroadcastMessage)) return;

            try
            {
                IsBusy = true;
                // We'll need to implement this in SignalRNotificationService
                await _signalRService.SendBroadcastMessageAsync("System Admin", BroadcastMessage);
                
                await _dialogService.ShowAlertAsync("Broadcast Sent", "Message has been sent to all active users.");
                BroadcastMessage = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send broadcast");
                await _dialogService.ShowAlertAsync("Error", $"Failed to send broadcast: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task SimulateApiUpdate()
        {
            BroadcastMessage = "API is updating. Please save your work and log out. System will be offline in 5 minutes.";
            await SendBroadcast();
        }
    }
}
