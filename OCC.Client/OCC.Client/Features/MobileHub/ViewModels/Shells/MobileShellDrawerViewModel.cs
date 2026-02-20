using System;

namespace OCC.Client.Features.MobileHub.ViewModels.Shells
{
    public partial class MobileShellDrawerViewModel : MobileShellViewModelBase
    {
        public MobileShellDrawerViewModel(
            MobileDashboardViewModel dashboardViewModel,
            MobileRollCallViewModel rollCallViewModel,
            Action onExit)
            : base(dashboardViewModel, rollCallViewModel, onExit)
        {
        }
    }
}
