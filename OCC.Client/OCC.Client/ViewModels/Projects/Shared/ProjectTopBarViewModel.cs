using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using OCC.Client.ViewModels.Core;
using OCC.Client.Infrastructure;

namespace OCC.Client.ViewModels.Projects.Shared
{
    public partial class ProjectTopBarViewModel : ViewModelBase, IRecipient<ProjectVariationCountChangedMessage>
    {
        #region Private Members

        private readonly OCC.Client.Services.Interfaces.IPermissionService _permissionService;

        #endregion

        public bool CanAccessCalendar => _permissionService != null && _permissionService.CanAccess(NavigationRoutes.Calendar);
        public bool CanDeleteProject => _permissionService != null && _permissionService.CanAccess(NavigationRoutes.Feature_ProjectDeletion);

        public ProjectTopBarViewModel(OCC.Client.Services.Interfaces.IPermissionService permissionService)
        {
            _permissionService = permissionService;
            WeakReferenceMessenger.Default.Register(this);
        }

        public ProjectTopBarViewModel()
        {
             _permissionService = null!;
        }

        #region Events

        public event System.EventHandler? EditProjectRequested;
        public event System.EventHandler? DeleteProjectRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private string _activeTab = "Dashboard";

        [ObservableProperty]
        private string _projectName = "Engen";

        [ObservableProperty]
        private string _projectIconInitials = "OR";

        [ObservableProperty]
        private System.Guid _projectId;

        [ObservableProperty]
        private string _projectAddress = string.Empty;

        [ObservableProperty]
        private int _openVariationCount;

        #endregion

        #region Methods

        public void Receive(ProjectVariationCountChangedMessage message)
        {
            if (message.ProjectId == ProjectId)
            {
                OpenVariationCount = message.Count;
            }
        }

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
            EditProjectRequested?.Invoke(this, System.EventArgs.Empty);
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
