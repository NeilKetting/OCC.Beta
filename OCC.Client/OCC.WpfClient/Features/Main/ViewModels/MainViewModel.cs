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
        private bool _isSidebarMinimized;

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

            InitializeNavigation();
            
            // Setup CollectionView filtering/grouping
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(NavigationItems);
            view.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription(nameof(NavItem.Category)));

            // Open Dashboard by default - Removed to start blank as requested
            // OpenHub<DashboardViewModel>();
        }

        private void InitializeNavigation()
        {
            var items = new List<NavItem>
            {
                new NavItem("Dashboard", "", NavigationRoutes.Home, "Main", true),
                new NavItem("Global Calendar", "", NavigationRoutes.Calendar, "Main"),
                new NavItem("Wages Hub", "", NavigationRoutes.Feature_Wages, "Main"),
                new NavItem("Inventory", "", NavigationRoutes.Receiving, "Main"),
                new NavItem("Projects", "", NavigationRoutes.Projects, "Main"),
                
                new NavItem("Employees", "", NavigationRoutes.StaffManagement, "Administration"),
                new NavItem("Settings", "", NavigationRoutes.CompanySettings, "Administration"),
            };

            foreach (var item in items)
            {
                if (_permissionService.CanAccess(item.Route))
                {
                    NavigationItems.Add(item);
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
            
            // Map routes to ViewModels
            switch (item.Route)
            {
                case NavigationRoutes.Home:
                    OpenHub<DashboardViewModel>();
                    break;
                case NavigationRoutes.StaffManagement:
                    OpenHub<EmployeeListViewModel>();
                    break;
            }

            // Sync sidebar state
            foreach (var nav in NavigationItems) nav.IsActive = (nav == item);
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
        public string Icon { get; }
        public string Route { get; }
        public string Category { get; }

        [ObservableProperty]
        private bool _isActive;

        public NavItem(string label, string icon, string route, string category, bool isActive = false)
        {
            Label = label;
            Icon = icon;
            Route = route;
            Category = category;
            IsActive = isActive;
        }
    }
}
