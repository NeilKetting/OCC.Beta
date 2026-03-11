using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;
using OCC.WpfClient.Features.Employees.ViewModels;

namespace OCC.WpfClient.Features.Main.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private INavigationService _navigation;

        [ObservableProperty]
        private string _userName = "Neil Ketting";

        [ObservableProperty]
        private string _userEmail = "neil@mdk.co.za";

        [ObservableProperty]
        private ObservableCollection<NavItem> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<ViewModelBase> _openHubs = new();

        private ViewModelBase? _activeHub;
        public ViewModelBase? ActiveHub
        {
            get => _activeHub;
            set
            {
                if (SetProperty(ref _activeHub, value))
                {
                    foreach (var hub in OpenHubs)
                    {
                        hub.IsActiveHub = (hub == value);
                    }
                }
            }
        }

        [ObservableProperty]
        private bool _isSidebarMinimized = true;

        [ObservableProperty]
        private string _userActivityStatus = "Ready";

        [ObservableProperty]
        private string _dbStatusText = "Connected";

        [ObservableProperty]
        private bool _isDbConnected = true;

        [ObservableProperty]
        private string _onlineCount = "1"; // Just matching type if it was int

        [ObservableProperty]
        private string _currentTime;

        [ObservableProperty]
        private string _currentDate;

        private readonly System.Windows.Threading.DispatcherTimer _clockTimer;

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarMinimized = !IsSidebarMinimized;
        }

        public MainViewModel(INavigationService navigation, IPermissionService permissionService, IServiceProvider serviceProvider)
        {
            _navigation = navigation;
            _permissionService = permissionService;
            _serviceProvider = serviceProvider;
            Title = "Main Shell";
            
            // Start minimized as requested
            IsSidebarMinimized = true;

            InitializeNavigation();
            
            // Setup CollectionView filtering/grouping
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(NavigationItems);
            view.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription(nameof(NavItem.Category)));

            _clockTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => UpdateTime();
            _clockTimer.Start();
            UpdateTime(); // initial call
            
            // Open Dashboard by default - Removed to start blank as requested
            // OpenHub<DashboardViewModel>();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            CurrentTime = now.ToString("HH:mm:ss");
            CurrentDate = now.ToString("dddd, d") + GetDaySuffix(now.Day) + now.ToString(" MMMM yyyy");
        }

        private string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        private void InitializeNavigation()
        {
            var items = new List<NavItem>
            {
                new NavItem("Dashboard", "IconHome", NavigationRoutes.Home, "Main"),
                new NavItem("Chat", "IconChat", NavigationRoutes.Chat, "Main"),
                new NavItem("Global Calendar", "IconCalendar", NavigationRoutes.Calendar, "Main"),
                
                new NavItem("Time & Attendance", "IconTime", string.Empty, "Main")
                {
                    Children = 
                    {
                        new NavItem("Live Attendance", "IconLiveView", NavigationRoutes.AttendanceLive, "Main"),
                        new NavItem("Clock History", "IconHistory", NavigationRoutes.AttendanceHistory, "Main"),
                        new NavItem("Leave Application", "IconInformation", string.Empty, "Main"),
                        new NavItem("Leave Approvals", "IconCheck", string.Empty, "Main"),
                        new NavItem("Overtime Request", "IconHistory", string.Empty, "Main"),
                        new NavItem("Overtime Approval", "IconOvertimeApproved", string.Empty, "Main")
                    }
                },

                new NavItem("Wages", "IconWagesDollar", string.Empty, "Main")
                {
                    Children =
                    {
                        new NavItem("Wage Run", "IconWagesDollar", NavigationRoutes.Feature_Wages, "Main"),
                        new NavItem("Loans", "IconBank", NavigationRoutes.Feature_Wages, "Main") // Adjust routes as needed later
                    }
                },

                new NavItem("Settings", "IconGear", string.Empty, "Main")
                {
                    Children =
                    {
                        new NavItem("User Preferences", "IconCompanyProfile", string.Empty, "Main"),
                        new NavItem("Alerts", "IconAlertCircle", string.Empty, "Main")
                    }
                },

                new NavItem("Projects", "IconPortfolio", NavigationRoutes.Projects, "Main"),

                new NavItem("Orders", "IconDelivery", string.Empty, "Main")
                {
                    Children =
                    {
                        new NavItem("Suppliers", "IconTeam", string.Empty, "Main"),
                        new NavItem("Purchase Orders", "IconFile", string.Empty, "Main"),
                        new NavItem("Receive Stock", "IconDelivery", string.Empty, "Main"),
                        new NavItem("Picking Slip", "IconList", string.Empty, "Main")
                    }
                },

                new NavItem("HSEQ", "IconHealthSafety", string.Empty, "Main")
                {
                    Children =
                    {
                        new NavItem("Dashboard", "IconHome", string.Empty, "Main"),
                        new NavItem("Capture Audit", "IconAudit", string.Empty, "Main"),
                        new NavItem("Training and Medicals", "IconTeam", string.Empty, "Main"),
                        new NavItem("Report Incident", "IconAlertCircle", string.Empty, "Main"),
                        new NavItem("Documents", "IconFile", string.Empty, "Main")
                    }
                },

                new NavItem("Admin", "IconGear", string.Empty, "Administration")
                {
                    Children =
                    {
                        new NavItem("Company Profile", "IconCompanyProfile", string.Empty, "Administration"),
                        new NavItem("Users", "IconTeam", string.Empty, "Administration"),
                        new NavItem("Employees", "IconTeam", NavigationRoutes.StaffManagement, "Administration"),
                        new NavItem("System Settings", "IconGear", NavigationRoutes.CompanySettings, "Administration")
                    }
                }
            };

            foreach (var item in items)
            {
                // Top-level permission check or just add directly if route is empty/parent
                if (string.IsNullOrEmpty(item.Route) || _permissionService.CanAccess(item.Route))
                {
                    // Filter children by permissions
                    var accessibleChildren = item.Children.Where(c => string.IsNullOrEmpty(c.Route) || _permissionService.CanAccess(c.Route)).ToList();
                    
                    item.Children.Clear();
                    foreach (var child in accessibleChildren)
                    {
                        item.Children.Add(child);
                    }

                    // Only add parent if it has children, or if it's a standalone endpoint
                    if (item.IsParent || !string.IsNullOrEmpty(item.Route))
                    {
                        NavigationItems.Add(item);
                    }
                }
            }
        }

        [RelayCommand]
        private void Logout()
        {
            Navigation.NavigateTo<AuthHub.ViewModels.AuthViewModel>();
        }

        [RelayCommand]
        private void Navigate(NavItem item)
        {
            if (item == null) return;
            
            // If it's a parent node, just expand/collapse it
            if (item.IsParent)
            {
                item.IsExpanded = !item.IsExpanded;
                return;
            }

            if (string.IsNullOrEmpty(item.Route)) return;
            
            // Map routes to ViewModels
            switch (item.Route)
            {
                case NavigationRoutes.Home:
                    OpenHub<DashboardViewModel>();
                    break;
                case NavigationRoutes.Chat:
                    OpenHub<OCC.WpfClient.Features.Chat.ViewModels.ChatViewModel>();
                    break;
                case NavigationRoutes.StaffManagement:
                    OpenHub<EmployeeListViewModel>();
                    break;
            }

            // Sync sidebar state
            foreach (var current in NavigationItems)
            {
                current.IsActive = current == item;
                if (current.IsParent)
                {
                    foreach (var child in current.Children)
                    {
                        child.IsActive = child == item;
                    }
                }
            }
        }

        [RelayCommand]
        private void CloseHub(ViewModelBase hub)
        {
            if (hub is DashboardViewModel) return; // Prevent closing dashboard for now
            
            OpenHubs.Remove(hub);
            if (ActiveHub == hub)
            {
                ActiveHub = OpenHubs.LastOrDefault();
            }
        }

        [RelayCommand]
        private void NavigateToHub(ViewModelBase hub)
        {
            ActiveHub = hub;
        }

        private void OpenHub<T>() where T : ViewModelBase
        {
            var existing = OpenHubs.OfType<T>().FirstOrDefault();
            if (existing != null)
            {
                ActiveHub = existing;
            }
            else
            {
                var hub = _serviceProvider.GetRequiredService<T>();
                OpenHubs.Add(hub);
                ActiveHub = hub;
            }
        }
    }

    public partial class NavItem : ObservableObject
    {
        public string Label { get; }
        public System.Windows.Media.Geometry? Icon { get; }
        public string Route { get; }
        public string Category { get; }

        public ObservableCollection<NavItem> Children { get; } = new();
        public bool IsParent => Children.Any();

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isExpanded;

        public NavItem(string label, string iconKey, string route, string category, bool isActive = false)
        {
            Label = label;
            Icon = System.Windows.Application.Current?.TryFindResource(iconKey) as System.Windows.Media.Geometry;
            Route = route;
            Category = category;
            IsActive = isActive;
        }
    }
}
