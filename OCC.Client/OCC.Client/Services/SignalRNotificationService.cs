using Microsoft.AspNetCore.SignalR.Client;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class SignalRNotificationService : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        public event Action<string>? OnNotificationReceived;

        public SignalRNotificationService()
        {
            // TODO: Move URL to AppSettings
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7193/hubs/notifications") // Adjust port as needed
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("ReceiveNotification", (message) =>
            {
                OnNotificationReceived?.Invoke(message);
            });

            _hubConnection.On<string, string, Guid>("EntityUpdate", (entityType, action, id) =>
            {
                // Dispatch to UI thread if needed, but Messenger handles logic. 
                // Using WeakReferenceMessenger to broadcast
                var msg = new ViewModels.Messages.EntityUpdatedMessage(entityType, action, id);
                WeakReferenceMessenger.Default.Send(msg);
            });
        }

        public async Task StartAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // Handle connection errors (log them)
                System.Diagnostics.Debug.WriteLine($"SignalR Connection Failed: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
