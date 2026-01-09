using System;
using Avalonia.Input;
using Avalonia.Input.Raw; // Keeping for now if needed, but likely removing
using Avalonia.Controls; // Added for TopLevel
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OCC.Client.Services.Infrastructure
{
    public partial class UserActivityService : ObservableObject, IDisposable
    {
        private readonly DispatcherTimer _idleTimer;
        private DateTime _lastActivity = DateTime.Now;
        private const int IdleThresholdMinutes = 1; // Testing: 1 min. Real: 5+

        [ObservableProperty]
        private bool _isAway;

        [ObservableProperty]
        private string _statusText = "Active";

        public UserActivityService()
        {
            _idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _idleTimer.Tick += CheckIdleStatus;
            _idleTimer.Start();
        }

        public void Monitor(Avalonia.Controls.TopLevel topLevel)
        {
            if (topLevel != null)
            {
                topLevel.AddHandler(InputElement.PointerMovedEvent, OnInputActivity, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                topLevel.AddHandler(InputElement.KeyDownEvent, OnInputActivity, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            }
        }

        private void OnInputActivity(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _lastActivity = DateTime.Now;
            if (IsAway)
            {
                IsAway = false;
                StatusText = "Active";
            }
        }

        private void CheckIdleStatus(object? sender, EventArgs e)
        {
            var idleTime = DateTime.Now - _lastActivity;
            if (idleTime.TotalMinutes >= IdleThresholdMinutes)
            {
                IsAway = true;
                string timeString = idleTime.TotalHours >= 1 
                    ? $"{(int)idleTime.TotalHours}h {idleTime.Minutes}m" 
                    : $"{idleTime.Minutes}m";
                
                StatusText = $"Away ({timeString})";
            }
        }

        public void Dispose()
        {
            _idleTimer.Stop();
        }
    }
}
