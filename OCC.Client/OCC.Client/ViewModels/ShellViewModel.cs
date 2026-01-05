using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OCC.Client.ViewModels.Home;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.EmployeeManagement;
using System;

namespace OCC.Client.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private SidebarViewModel _sidebar;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        public ShellViewModel(IServiceProvider serviceProvider, SidebarViewModel sidebar)
        {
            _serviceProvider = serviceProvider;
            Sidebar = sidebar;

            // Default to Home (Dashboard)
            NavigateTo("Home");

            Sidebar.PropertyChanged += Sidebar_PropertyChanged;
        }

        private void Sidebar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.ActiveSection))
            {
                NavigateTo(Sidebar.ActiveSection);
            }
        }

        private void NavigateTo(string section)
        {
            switch (section)
            {
                case "Home":
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
                case "Team": // "Staff Management"
                    CurrentPage = _serviceProvider.GetRequiredService<EmployeeManagementViewModel>();
                    break;
                case "Portfolio": // "Projects"
                    CurrentPage = _serviceProvider.GetRequiredService<ProjectsViewModel>();
                    break;
                case "Time":
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Time.TimeViewModel>();
                    break;
                case "Calendar": 
                    // Assuming accessing Calendar via Sidebar (if implemented) or other means
                    CurrentPage = _serviceProvider.GetRequiredService<ViewModels.Home.Calendar.CalendarViewModel>();
                    break;
                 default:
                    CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
                    break;
            }
        }
    }
}
