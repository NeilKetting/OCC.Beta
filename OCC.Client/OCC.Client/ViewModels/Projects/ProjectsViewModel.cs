using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Projects.Shared;
using OCC.Client.ViewModels.Home.ProjectSummary;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectsViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<ProjectSelectedMessage>
    {
        private readonly ProjectDetailViewModel _projectDetailVM;
        private readonly ProjectSummaryViewModel _projectSummaryVM;
        private readonly ProjectListViewModel _projectListVM;

        private readonly ProjectReportViewModel _projectReportVM;

        [ObservableProperty]
        private ProjectMainMenuViewModel _projectMainMenu;

        [ObservableProperty]
        private ViewModelBase _currentView;

        public ProjectsViewModel(
            ProjectMainMenuViewModel projectMenu, 
            ProjectListViewModel projectsListVM,
            ProjectDetailViewModel projectDetailVM,
            ProjectSummaryViewModel projectSummaryVM,
            ProjectReportViewModel projectReportVM) 
        {
            _projectMainMenu = projectMenu;
            _projectListVM = projectsListVM;
            _projectDetailVM = projectDetailVM;
            _projectSummaryVM = projectSummaryVM;
            _projectReportVM = projectReportVM;

            // Default
            _currentView = _projectListVM;

            _projectMainMenu.PropertyChanged += Menu_PropertyChanged;

            // Register for Project Selection
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void Menu_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectMainMenuViewModel.ActiveTab))
            {
                UpdateView();
            }
        }

        private void UpdateView()
        {
            switch (_projectMainMenu.ActiveTab)
            {
                case "Dashboard":
                    CurrentView = _projectSummaryVM;
                    break;
                case "Reports":
                    CurrentView = _projectReportVM; // Use the new Report View
                    break;
                case "Projects":
                default:
                    CurrentView = _projectListVM;
                    break;
            }
        }

        public void Receive(ProjectSelectedMessage message)
        {
             // Switch to Detail View
             CurrentView = _projectDetailVM;
             _projectDetailVM.LoadTasks(message.Value.Id);
        }
    }
}
