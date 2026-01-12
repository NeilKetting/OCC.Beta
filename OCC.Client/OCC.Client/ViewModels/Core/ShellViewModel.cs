using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.HealthSafety;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.EmployeeManagement;
using OCC.Client.ViewModels.Settings;
using OCC.Client.ViewModels; // For LoginViewModel
using OCC.Client.ViewModels.Shared; // For ProfileViewModel
using OCC.Client.ViewModels.Messages; // Corrected Namespace
using OCC.Client.Services;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using System.Linq;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.Services.Infrastructure;
using OCC.Client.Messages; // NEW
using OCC.Client.ViewModels.Notifications;
using OCC.Client.ViewModels.Core;
using OCC.Client.Views.Core; // Added
using OCC.Client.Views.Login; // Added
// using OCC.Client.Views.Login; // Removed Duplicate
using OCC.Client.ViewModels.Login; // Added
using OCC.Shared.Models; // Added for Employee

namespace OCC.Client.ViewModels.Core
{
    public partial class ShellViewModel : ViewModelBase, 
        IRecipient<OpenNotificationsMessage>,
        IRecipient<OpenManageUsersMessage>,
        IRecipient<TestBirthdayMessage>,
        IRecipient<ToastNotificationMessage>
    {

        #region Private Members

        private readonly IServiceProvider _serviceProvider;
        private readonly IPermissionService _permissionService;
        private readonly SignalRNotificationService _signalRService;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;

        #endregion

        #region Observables

        [ObservableProperty]
        private SideMenuViewModel _sideMenuViewModel;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        [ObservableProperty]
        private NotificationViewModel _notificationVM;

        [ObservableProperty]
        private bool _isNotificationOpen;

        [ObservableProperty]
        private bool _isAuthenticated;

        [ObservableProperty]
        private int _onlineCount;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<UserDisplayModel> _connectedUsers = new();

        [ObservableProperty]
        private string _userActivityStatus = "Active";

        public System.Collections.ObjectModel.ObservableCollection<Models.ToastMessage> Toasts { get; } = new();

        #endregion

        #region Constructors

        public ShellViewModel()
        {
            // Parameterless constructor for design-time support
            _serviceProvider = null!;
            _permissionService = null!;
            _sideMenuViewModel = null!;
            _currentPage = null!;
            _signalRService = null!;
            _notificationVM = null!;
            _authService = null!;
            _dialogService = null!;
            _toastService = null!;
        }

        public ShellViewModel(
            IServiceProvider serviceProvider, 
            SideMenuViewModel sideMenuViewModel, 
            IUpdateService updateService, 
            IPermissionService permissionService,
            SignalRNotificationService signalRService,
            UserActivityService userActivityService,
            IDialogService dialogService,
            IToastService toastService,
            IAuthService authService)
        {
            _serviceProvider = serviceProvider;
            _permissionService = permissionService;
            _signalRService = signalRService;
            _authService = authService;
            _dialogService = dialogService;
            _toastService = toastService;
            
            _signalRService.OnUserListReceived += OnUserUiUpdate;
            _signalRService.OnBroadcastReceived += (sender, message) => 
            {
                _toastService.ShowInfo($"Broadcast from {sender}", message);
            };

            // Subscribe to Navigation Requests
            WeakReferenceMessenger.Default.Register<NavigationRequestMessage>(this, (r, m) =>
            {
                HandleNavigationRequest(m.Value, m.Payload);
            });
            
            // User Activity
            UserActivityStatus = userActivityService.StatusText;
            userActivityService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UserActivityService.StatusText))
                {
                    UserActivityStatus = userActivityService.StatusText;
                }
            };
            userActivityService.SessionWarning += OnSessionWarning;
            userActivityService.SessionExpired += OnSessionExpired;

            _sideMenuViewModel = sideMenuViewModel;
            _sideMenuViewModel.PropertyChanged += SideMenu_PropertyChanged;

            _currentPage = null!; // Silence warning as NavigateTo sets it
            
            // Initialize persistent Notification ViewModel
            _notificationVM = _serviceProvider.GetRequiredService<NotificationViewModel>();

            // Default to Home (Dashboard) unless Beta Notice is pending
            var currentVersion = updateService.CurrentVersion;
            
            if (!DevelopmentToBeDeleted.BetaNoticeViewModel.IsNoticeAccepted(currentVersion))
            {
                var betaVM = new DevelopmentToBeDeleted.BetaNoticeViewModel(currentVersion);
                betaVM.Accepted += () => 
                {
                    NavigateTo(Infrastructure.NavigationRoutes.Home);
                };
                betaVM.OpenReleaseNotesRequested += () =>
                {
                    var releaseNotesVM = new ViewModels.Help.ReleaseNotesViewModel();
                    releaseNotesVM.CloseRequested += (s, e) => 
                    {
                        // Return to Beta Notice
                        CurrentPage = betaVM;
                    };
                    CurrentPage = releaseNotesVM;
                };
                CurrentPage = betaVM;
            }
            else
            {
                NavigateTo(Infrastructure.NavigationRoutes.Home);
            }

            WeakReferenceMessenger.Default.RegisterAll(this); // Register for messages

            // Start/Restart SignalR Connection Globally to ensure Auth Token is used
            _ = _signalRService.RestartAsync();

            // Auto-Update removed from here - moved to App Startup
            
            // Check Birthdays
            CheckBirthdaysAsync(serviceProvider.GetRequiredService<IRepository<Employee>>());
        }

        private async void CheckBirthdaysAsync(IRepository<Employee> employeeRepository)
        {
             // Run on background initially but Dialog must be on UI
             await Task.Delay(2000); // Wait for things to settle

             try
             {
                 var today = DateTime.Today;
                 var employees = await employeeRepository.GetAllAsync();
                 var birthdayPeople = employees.Where(e => e.Status == EmployeeStatus.Active && 
                                                      e.DoB.Date.Month == today.Month && 
                                                      e.DoB.Date.Day == today.Day).ToList();

                 if (!birthdayPeople.Any()) return;

                 var currentUser = _authService.CurrentUser;
                 
                 foreach (var person in birthdayPeople)
                 {
                     // Personal Wish
                     // Check if this person IS the current user
                     // We link by LinkedUserId
                     if (currentUser != null && person.LinkedUserId == currentUser.Id)
                     {
                         if (currentUser.UserRole == UserRole.Admin || currentUser.UserRole == UserRole.Office)
                         {
                             // Professional Wish Popup
                             await _dialogService.ShowAlertAsync("Happy Birthday! 🎂", 
                                 $"Dear {person.FirstName},\n\n" +
                                 "Wishing you a fantastic birthday filled with success and happiness.\n" +
                                 "Thank you for your hard work and dedication!\n\n" +
                                 "Best Regards,\n OCC Management");
                         }
                     }
                 }
                 
                 // Send General Notifications for valid birthdays
                 // Make sure loop above didn't block.
                 // We'll just populate the Notification Center
                 var names = string.Join(", ", birthdayPeople.Select(b => b.FirstName));
                 if (!string.IsNullOrEmpty(names))
                 {
                     NotificationVM.AddSystemNotification("Birthdays", $"Happy Birthday to: {names} 🎂");
                 }
             }
             catch (Exception ex)
             {
                 // Ignore
                 System.Diagnostics.Debug.WriteLine($"Birthday check failed: {ex.Message}");
             }
        }

        private async void OnSessionWarning(object? sender, EventArgs e)
        {
            var result = await _dialogService.ShowSessionTimeoutAsync();
            if (!result)
            {
                // User didn't click "Yes" (Timed out or closed)
                PerformLogout();
            }
            // If result is true, UserActivityService handles the "Active" logic via input detection.
        }

        private void OnSessionExpired(object? sender, EventArgs e)
        {
            // Failsafe if dialog didn't close or wasn't shown
            PerformLogout();
        }

        private void PerformLogout()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                // 1. Clear Auth
                await _authService.LogoutAsync(); 
                
                // 2. Stop SignalR
                // _signalRService.StopAsync(); // Optional, but good practice

                // 3. Navigate to Login via proper Navigation Message
                var loginVM = _serviceProvider.GetRequiredService<LoginViewModel>();
                WeakReferenceMessenger.Default.Send(new NavigationMessage(loginVM));
            });
        }


        [RelayCommand]
        public async Task ReportBug()
        {
            var viewName = "Unknown";

            // 1. Check for any open Popup/Dialog windows first
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Find visible windows that are NOT the main window or splash screen
                var popup = desktop.Windows
                    .LastOrDefault(w => w.IsVisible && 
                                        w.GetType().Name != "MainWindow" && 
                                        w.GetType().Name != "SplashView" &&
                                        w.GetType().Name != "BugReportDialog"); // Don't report the bug dialog itself if somehow double-triggered

                if (popup != null)
                {
                    viewName = popup.GetType().Name.Replace("View", "");
                    // Optional: Append ViewModel name for clarity
                    if (popup.DataContext != null)
                    {
                        var vmName = popup.DataContext.GetType().Name.Replace("ViewModel", "");
                        viewName += $" ({vmName})";
                    }
                }
            }

            // 2. Fallback to CurrentPage if no popup found or viewName is still default
            if (viewName == "Unknown")
            {
                if (CurrentPage is ViewModels.Projects.ProjectsViewModel projectsVM && projectsVM.CurrentView != null)
                {
                    viewName = projectsVM.CurrentView.GetType().Name.Replace("ViewModel", "View");
                }
                else if (CurrentPage is ViewModels.EmployeeManagement.EmployeeManagementViewModel empVM)
                {
                    if (empVM.IsAddEmployeePopupVisible) viewName = "EmployeeDetailView";
                    else if (empVM.IsAddTeamPopupVisible) viewName = "TeamDetailView";
                }
                else if (CurrentPage is ViewModels.Settings.UserManagementViewModel userVM && userVM.IsUserPopupVisible)
                {
                    viewName = "UserDetailView";
                }
                else if (CurrentPage is ViewModels.Orders.OrderViewModel orderVM)
                {
                    if (orderVM.IsOrderDetailVisible) viewName = "OrderDetailView";
                    else if (orderVM.IsReceiveOrderVisible) viewName = "ReceiveOrderView";
                    else if (orderVM.IsSupplierDetailVisible) viewName = "SupplierDetailView";
                    else if (orderVM.CurrentView != null) viewName = orderVM.CurrentView.GetType().Name.Replace("ViewModel", "View");
                }
                else if (CurrentPage is ViewModels.Home.Calendar.CalendarViewModel calVM && calVM.IsCreatePopupVisible)
                {
                    viewName = "CreateTaskPopupView";
                }
                else if (CurrentPage is ViewModels.Time.TimeAttendanceViewModel timeVM)
                {
                    if (timeVM.CurrentView != null)
                    {
                        viewName = timeVM.CurrentView.GetType().Name.Replace("ViewModel", "View");
                    }
                    else
                    {
                        viewName = "TimeAttendanceView";
                    }
                }
                else if (CurrentPage is ViewModels.Time.TimeLiveViewModel timeLiveVM && timeLiveVM.IsTimesheetVisible)
                {
                    viewName = "DailyTimesheetView";
                }
                else
                {
                    viewName = CurrentPage?.GetType().Name.Replace("ViewModel", "View") ?? "ShellView";
                    if (CurrentPage is ViewModels.Home.HomeViewModel) viewName = "Dashboard";
                }
            }
            
            await _dialogService.ShowBugReportAsync(viewName);
        }

        [RelayCommand]
        public async Task TestBirthday()
        {
             var currentUser = _authService.CurrentUser;
             var name = currentUser?.FirstName ?? "User";
             
             // Professional Wish Popup Simulation
             await _dialogService.ShowAlertAsync("Happy Birthday! 🎂", 
                 $"Dear {name},\n\n" +
                 "Wishing you a fantastic birthday filled with success and happiness.\n" +
                 "Thank you for your hard work and dedication!\n\n" +
                 "Best Regards,\n OCC Management");
                 
             // Also simulate notification
             NotificationVM.AddSystemNotification("Birthdays", $"Happy Birthday to: {name} 🎂");
        }

        #endregion

        #region Helper Methods

        private void SideMenu_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SideMenuViewModel.ActiveSection))
            {
                NavigateTo(SideMenuViewModel.ActiveSection);
            }
        }

        private void NavigateTo(string section)
        {
            // Close notification popup when navigating
            IsNotificationOpen = false;

            if (!_permissionService.CanAccess(section))
            {
                if (section != Infrastructure.NavigationRoutes.Home)
                {
                    NavigateTo(Infrastructure.NavigationRoutes.Home);
                }
                return;
            }

            switch (section)
            {
                case Infrastructure.NavigationRoutes.Home:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.StaffManagement:
                    CurrentPage = _serviceProvider.GetRequiredService<EmployeeManagementViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Projects:
                    CurrentPage = _serviceProvider.GetRequiredService<ProjectsViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Time:
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Time.TimeAttendanceViewModel>();
                    break;
                case Infrastructure.NavigationRoutes.Calendar: 
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Home.Calendar.CalendarViewModel>();
                    break;
                case "UserManagement":
                    CurrentPage = _serviceProvider.GetRequiredService<UserManagementViewModel>();
                    break;
                case "MyProfile":
                    CurrentPage = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    break;
                case "HealthSafety":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.HealthSafety.HealthSafetyViewModel>();
                    break;
                case "Orders":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Orders.OrderViewModel>();
                    break;
                case "CreateOrder":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Orders.CreateOrderViewModel>();
                    break;
                case "Help":
                     var releaseNotesVM = new ViewModels.Help.ReleaseNotesViewModel();
                     releaseNotesVM.CloseRequested += (s, e) => NavigateTo(Infrastructure.NavigationRoutes.Home);
                     CurrentPage = releaseNotesVM;
                     break;
                case "AuditLog":
                    CurrentPage = _serviceProvider.GetRequiredService<AuditLogViewModel>();
                    break;
                case "CompanySettings":
                    CurrentPage = _serviceProvider.GetRequiredService<CompanySettingsViewModel>();
                    break;
                case "UserPreferences":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Settings.UserPreferencesViewModel>();
                    break;
                case "BugList":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Bugs.BugListViewModel>();
                    break;
                case "Developer":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Developer.DeveloperViewModel>();
                    break;
                 default:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
            }
        }

        public void Receive(OpenNotificationsMessage message)
        {
            // Toggle visibility
            IsNotificationOpen = !IsNotificationOpen;
        }

        private void HandleNavigationRequest(string route, object? payload)
        {
            if (route == "InventoryDetail_Create")
            {
                // This is a special case where we want to open a DETAIL view for creation
                // Since our main navigation is usually page-based ("Orders", "Inventory"), 
                // we might need to navigate to InventoryView first, then trigger the detail popup?
                // OR we can just replace the CurrentPage with the DetailVM directly if that's supported.
                
                // Let's assume we can navigate to the Detail VM directly as a "Page" or Dialog.
                // Looking at InventoryDetailViewModel, is it a page or a dialog? 
                // It's used as a dialog/child in InventoryView usually. 
                // Let's try to simulate navigating to "Orders/Inventory" but forcing the detail view?
                
                // Better approach: Open InventoryDetailViewModel as the CurrentPage.
                var vm = _serviceProvider.GetRequiredService<ViewModels.Orders.InventoryDetailViewModel>();
                
                // Setup for creation with pre-filled SKU/Name
                if (payload is string searchTerm)
                {
                    // Heuristic: If it looks like a SKU (all caps, numbers), use as SKU. Else Name.
                    // For simplicity, let's put it in both or guess
                    var isSku = searchTerm.Any(char.IsDigit) && searchTerm.Any(char.IsUpper);
                    
                    vm.Load(null); // Clear/New
                    if (isSku) vm.Sku = searchTerm;
                    else vm.ProductName = searchTerm;
                }
                
                CurrentPage = vm;
            }
            else
            {
                NavigateTo(route);
            }
        }

        public void Receive(OpenManageUsersMessage message)
        {
            NavigateTo("UserManagement");
            
            if (message.Value.HasValue && CurrentPage is UserManagementViewModel vm)
            {
                vm.OpenUser(message.Value.Value);
            }
        }

        public async void Receive(TestBirthdayMessage message)
        {
            await TestBirthday();
        }

        public void Receive(ToastNotificationMessage message)
        {
            var toast = message.Value;
            Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                Toasts.Add(toast);
                
                // Auto-fade and remove after 10 seconds
                Task.Run(async () => {
                    await Task.Delay(8000); // Wait 8s then fade
                    for(int i=0; i<20; i++) {
                        await Task.Delay(100);
                        Avalonia.Threading.Dispatcher.UIThread.Post(() => toast.Opacity -= 0.05);
                    }
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => Toasts.Remove(toast));
                });
            });
        }

        [RelayCommand]
        public void CloseNotifications()
        {
            IsNotificationOpen = false;
        }

        private void OnUserUiUpdate(System.Collections.Generic.List<OCC.Shared.DTOs.UserConnectionInfo> users)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                // Check for new connections or disconnections to show toasts
                var currentNames = ConnectedUsers.Select(u => u.Name).ToList();
                var newNames = users.DistinctBy(u => u.UserName).Select(u => u.UserName).ToList();

                // New users
                var joined = newNames.Where(n => !currentNames.Contains(n)).ToList();
                foreach(var name in joined)
                {
                    if (name != _authService.CurrentUser?.Email) // Don't toast for self
                        _toastService.ShowInfo("User Online", $"{name} has connected.");
                }

                // Disconnected users
                var left = currentNames.Where(n => !newNames.Contains(n)).ToList();
                foreach(var name in left)
                {
                    _toastService.ShowWarning("User Offline", $"{name} has disconnected.");
                }

                ConnectedUsers.Clear();
                // Filter distinct users by UserName to avoid duplicates from multiple connections
                var distinctUsers = users.DistinctBy(u => u.UserName).OrderBy(u => u.UserName).ToList();
                
                foreach (var u in distinctUsers) 
                {
                    var timeOnline = DateTime.UtcNow - u.ConnectedAt;
                    var timeStr = timeOnline.TotalMinutes < 1 ? "Just now" : 
                                  timeOnline.TotalHours < 1 ? $"{timeOnline.Minutes}m" : 
                                  $"{timeOnline.Hours}h {timeOnline.Minutes}m";

                    var display = new UserDisplayModel(
                        u.UserName, 
                        timeStr, 
                        u.Status == "Away" ? Avalonia.Media.Brushes.Orange : Avalonia.Media.Brushes.Green
                    );
                    ConnectedUsers.Add(display);
                }
                OnlineCount = distinctUsers.Count;
            });
        }
        
        public record UserDisplayModel(string Name, string TimeOnline, Avalonia.Media.IBrush StatusColor);

        #endregion
    }
}
