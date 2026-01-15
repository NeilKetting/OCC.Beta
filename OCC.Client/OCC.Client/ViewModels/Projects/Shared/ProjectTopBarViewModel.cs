using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.Projects.Shared
{
    public partial class ProjectTopBarViewModel : ViewModelBase
    {
        #region Private Members

        private readonly OCC.Client.Services.Interfaces.IPermissionService _permissionService;

        #endregion

        public bool CanAccessCalendar => _permissionService != null && _permissionService.CanAccess("Calendar");

        public ProjectTopBarViewModel(OCC.Client.Services.Interfaces.IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public ProjectTopBarViewModel()
        {
             _permissionService = null!;
        }

        #region Events

        public event System.EventHandler? DeleteProjectRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _activeTab = "List";

        [ObservableProperty]
        private string _projectName = "Engen";

        [ObservableProperty]
        private string _projectIconInitials = "OR";

        [ObservableProperty]
        private int _trialDaysLeft = 25;

        [ObservableProperty]
        private System.Guid _projectId;

        #endregion

        #region Commands

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

        [RelayCommand]
        private void DeleteProject()
        {
            DeleteProjectRequested?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion
    }
}
