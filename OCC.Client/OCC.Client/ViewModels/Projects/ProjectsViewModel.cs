using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Projects.Shared;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectsViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<ProjectSelectedMessage>
    {
        private readonly ProjectDetailViewModel _projectDetailVM;

        [ObservableProperty]
        private ProjectMainMenuViewModel _projectMainMenu;

        [ObservableProperty]
        private ViewModelBase _currentView;

        public ProjectsViewModel(
            ProjectMainMenuViewModel projectMenu, 
            ProjectListViewModel projectsListVM,
            ProjectDetailViewModel projectDetailVM) // Inject Detail VM
        {
            _projectMainMenu = projectMenu;
            _currentView = projectsListVM;
            _projectDetailVM = projectDetailVM;

            // Register for Project Selection
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.RegisterAll(this);
        }

        public void Receive(ProjectSelectedMessage message)
        {
             // Switch to Detail View
             CurrentView = _projectDetailVM;
             _projectDetailVM.LoadTasks(message.Value.Id);
        }
    }
}
