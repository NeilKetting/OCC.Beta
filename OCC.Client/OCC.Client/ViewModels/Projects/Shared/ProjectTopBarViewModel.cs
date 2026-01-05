using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OCC.Client.ViewModels.Projects.Shared
{
    public partial class ProjectTopBarViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _activeTab = "List";

        [ObservableProperty]
        private string _projectName = "Engen";

        [ObservableProperty]
        private string _projectIconInitials = "OR";

        [ObservableProperty]
        private int _trialDaysLeft = 25;

        [RelayCommand]
        private void SetActiveTab(string tabName)
        {
            ActiveTab = tabName;
        }

        [RelayCommand]
        private void EditProject()
        {
            // TODO: Send message to open Edit Project dialog
        }

        [RelayCommand]
        private void ProjectSettings()
        {
             // TODO: Navigate to settings
        }

        [ObservableProperty]
        private System.Guid _projectId;

        public event System.EventHandler? DeleteProjectRequested;

        [RelayCommand]
        private void DeleteProject()
        {
            DeleteProjectRequested?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
