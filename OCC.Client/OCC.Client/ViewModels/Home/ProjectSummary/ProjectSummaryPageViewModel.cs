using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Home.Dashboard; // For TeamSummaryViewModel if it's there? Need to check.

namespace OCC.Client.ViewModels.Home.ProjectSummary
{
    public partial class ProjectSummaryPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ProjectSummaryViewModel _projectSummary;

        [ObservableProperty]
        private TeamSummaryViewModel _teamSummary; // Assuming this exists

        public ProjectSummaryPageViewModel(ProjectSummaryViewModel projectSummary, TeamSummaryViewModel teamSummary)
        {
            ProjectSummary = projectSummary;
            TeamSummary = teamSummary;
        }
    }
}
