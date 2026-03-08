using System;
using CommunityToolkit.Mvvm.ComponentModel;
using OCC.WpfClient.Infrastructure;

namespace OCC.WpfClient.Features.Main.ViewModels
{
    public partial class DashboardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _userName = "Neil Ketting";

        [ObservableProperty]
        private int _taskCount = 3;

        [ObservableProperty]
        private int _todoCount = 1;

        [ObservableProperty]
        private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

        [ObservableProperty]
        private string _greeting = "Good afternoon";

        public DashboardViewModel()
        {
            Title = "Dashboard";
        }
    }
}
