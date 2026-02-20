using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.ViewModels.Core;
using System;

namespace OCC.Client.Features.MobileHub.ViewModels.Shells
{
    public partial class MobileShellViewModelBase : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        private readonly MobileDashboardViewModel _dashboardViewModel;
        private readonly MobileRollCallViewModel _rollCallViewModel;
        private readonly Action _onExit;

        public MobileShellViewModelBase(
            MobileDashboardViewModel dashboardViewModel,
            MobileRollCallViewModel rollCallViewModel,
            Action onExit)
        {
            _dashboardViewModel = dashboardViewModel;
            _rollCallViewModel = rollCallViewModel;
            _onExit = onExit;
            _currentView = _dashboardViewModel;
        }

        [RelayCommand]
        private void NavigateToDashboard() => CurrentView = _dashboardViewModel;

        [RelayCommand]
        private void NavigateToRollCall() => CurrentView = _rollCallViewModel;

        [RelayCommand]
        private void ExitShell() => _onExit?.Invoke();
    }
}
