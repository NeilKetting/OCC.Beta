using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using OCC.Shared.Models;
using OCC.Client.Services;
using System;
using Avalonia.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Linq;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Infrastructure;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Notifications
{
    public partial class NotificationViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Notification> _notifications = new();

        [ObservableProperty]
        private bool _hasNotifications;

        [ObservableProperty]
        private bool _isAdmin;

        private readonly SignalRNotificationService _signalRService;
        private readonly IAuthService _authService;
        private readonly IRepository<User> _userRepository;

        public NotificationViewModel(SignalRNotificationService signalRService, IAuthService authService, IRepository<User> userRepository)
        {
            _signalRService = signalRService;
            _authService = authService;
            _userRepository = userRepository;
            
            // Check Admin Role
            IsAdmin = _authService.CurrentUser?.UserRole == UserRole.Admin;

            // Initialize with empty list for now
            HasNotifications = Notifications.Count > 0;
            Notifications.CollectionChanged += (s, e) => HasNotifications = Notifications.Count > 0;

            _signalRService.OnNotificationReceived += OnNotificationReceived;

            if (IsAdmin)
            {
                // Load pending approvals initially
                LoadPendingApprovals();
            }
        }

        public NotificationViewModel()
        {
             // Design time
             _signalRService = null!;
             _authService = null!;
             _userRepository = null!;
        }

        private async void LoadPendingApprovals()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var pending = users.Where(u => !u.IsApproved).ToList();
                
                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var user in pending)
                    {
                        // Check if we already have a notification for this user (simple dedup check)
                        if (!Notifications.Any(n => n.Message.Contains(user.Email)))
                        {
                            Notifications.Insert(0, new Notification
                            {
                                Title = "Approval Needed",
                                Message = $"New user awaiting approval: {user.FirstName} {user.LastName} ({user.Email})",
                                Timestamp = DateTime.Now, // Or user.DateCreated if available
                                IsRead = false
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading pending users: {ex}");
            }
        }

        private void OnNotificationReceived(string message)
        {
            // Logic for filtering notifications
            bool isRegistration = message.Contains("New Registration", StringComparison.OrdinalIgnoreCase) || 
                                  message.Contains("registered", StringComparison.OrdinalIgnoreCase);

            if (isRegistration)
            {
                // Only Admins care about new registrations
                if (!IsAdmin) return;

                // Bridge the gap: Notify UserManagementView to refresh
                WeakReferenceMessenger.Default.Send(new Messages.EntityUpdatedMessage("User", "Create", Guid.Empty));
            }
            else
            {
                // For other notifications (e.g. Task Assigned), check if it's for me
                // TODO: Parse message to see if it targets current user. For now, we assume other messages might be broadcast relevant.
                // If strictly "Task Assigned", the backend should ideally target the user ID. 
                // Since this is a simple string string hook, we'll just show it if it's not a registration message rejected by non-admin.
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Notifications.Insert(0, new Notification 
                { 
                    Title = isRegistration ? "New Registration" : "Notification", 
                    Message = message, 
                    Timestamp = DateTime.Now,
                    IsRead = false 
                });
            });
        }

        [RelayCommand]
        private async Task ApproveUser(Notification notification)
        {
            if (!IsAdmin) return;
            
            // Extract Email from message to find user
            // Message format: "New user awaiting approval: First Last (email)"
            // Simple extraction logic
            try 
            {
                var start = notification.Message.LastIndexOf('(');
                var end = notification.Message.LastIndexOf(')');
                
                if (start > -1 && end > start)
                {
                    var email = notification.Message.Substring(start + 1, end - start - 1);
                    var users = await _userRepository.GetAllAsync();
                    var user = users.FirstOrDefault(u => u.Email == email);
                    
                    if (user != null)
                    {
                        user.IsApproved = true;
                        await _userRepository.UpdateAsync(user);
                        
                        Notifications.Remove(notification);
                        
                        // Notify system
                        WeakReferenceMessenger.Default.Send(new Messages.EntityUpdatedMessage("User", "Update", user.Id));
                    }
                }
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error approving user from notification: {ex}");
            }
        }
        
        [RelayCommand]
        private void ClearNotification(Notification notification)
        {
            if (Notifications.Contains(notification))
            {
                Notifications.Remove(notification);
            }
        }
    }
}
