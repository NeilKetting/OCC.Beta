using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.Features.MobileHub.Models;
using OCC.Client.Features.MobileHub.ViewModels.Shells;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.Features.MobileHub.ViewModels
{
    public partial class MobileHubViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentView;

        private readonly MobileLayoutSelectorViewModel _layoutSelectorViewModel;
        private readonly MobileDashboardViewModel _dashboardViewModel;
        private readonly MobileRollCallViewModel _rollCallViewModel;

        public MobileHubViewModel(
            MobileDashboardViewModel dashboardViewModel,
            MobileRollCallViewModel rollCallViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _rollCallViewModel = rollCallViewModel;

            _layoutSelectorViewModel = new MobileLayoutSelectorViewModel(OnLayoutSelected);
            _currentView = _layoutSelectorViewModel;
        }

        private void OnLayoutSelected(MobileLayoutType layoutType)
        {
            switch (layoutType)
            {
                case MobileLayoutType.BottomNavigation:
                    CurrentView = new MobileShellBottomNavViewModel(_dashboardViewModel, _rollCallViewModel, ReturnToSelector);
                    break;
                case MobileLayoutType.SideDrawer:
                    CurrentView = new MobileShellDrawerViewModel(_dashboardViewModel, _rollCallViewModel, ReturnToSelector);
                    break;
                case MobileLayoutType.TabbedDashboard:
                    CurrentView = new MobileShellTabViewModel(_dashboardViewModel, _rollCallViewModel, ReturnToSelector);
                    break;
            }
        }

        private void ReturnToSelector()
        {
            CurrentView = _layoutSelectorViewModel;
        }
    }
}
