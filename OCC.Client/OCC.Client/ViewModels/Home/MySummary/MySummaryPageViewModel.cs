using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Home.Dashboard;

namespace OCC.Client.ViewModels.Home.MySummary
{
    public partial class MySummaryPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private SummaryViewModel _summary;

        [ObservableProperty]
        private TasksWidgetViewModel _tasksWidget;

        [ObservableProperty]
        private PulseViewModel _pulse;

        public MySummaryPageViewModel(SummaryViewModel summary, TasksWidgetViewModel tasksWidget, PulseViewModel pulse)
        {
            Summary = summary;
            TasksWidget = tasksWidget;
            Pulse = pulse;
        }
    }
}
