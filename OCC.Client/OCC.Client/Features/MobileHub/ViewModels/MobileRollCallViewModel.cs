using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.Features.MobileHub.ViewModels
{
    public partial class MobileRollCallViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = "Roll Call";

        public MobileRollCallViewModel()
        {
        }
    }
}
