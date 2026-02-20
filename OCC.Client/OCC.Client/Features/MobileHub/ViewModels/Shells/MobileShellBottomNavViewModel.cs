using System;

namespace OCC.Client.Features.MobileHub.ViewModels.Shells
{
    public partial class MobileShellBottomNavViewModel : MobileShellViewModelBase
    {
        public MobileShellBottomNavViewModel(
            MobileDashboardViewModel dashboardViewModel,
            MobileRollCallViewModel rollCallViewModel,
            Action onExit)
            : base(dashboardViewModel, rollCallViewModel, onExit)
        {
        }
    }
}
