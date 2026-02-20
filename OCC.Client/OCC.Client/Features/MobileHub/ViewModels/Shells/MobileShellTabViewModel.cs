using System;

namespace OCC.Client.Features.MobileHub.ViewModels.Shells
{
    public partial class MobileShellTabViewModel : MobileShellViewModelBase
    {
        public MobileShellTabViewModel(
            MobileDashboardViewModel dashboardViewModel,
            MobileRollCallViewModel rollCallViewModel,
            Action onExit)
            : base(dashboardViewModel, rollCallViewModel, onExit)
        {
        }
    }
}
