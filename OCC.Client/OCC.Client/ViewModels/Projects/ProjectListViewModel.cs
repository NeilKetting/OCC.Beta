using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.ViewModels.Projects.Dashboard;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectListViewModel : ViewModelBase
    {
        private readonly IProjectManager _projectManager;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private ObservableCollection<ProjectDashboardItemViewModel> _projects = new();

        [ObservableProperty]
        private ProjectDashboardItemViewModel? _selectedProject;

        public ProjectListViewModel(IProjectManager projectManager, IDialogService dialogService)
        {
            _projectManager = projectManager;
            _dialogService = dialogService;
        }

        public ProjectListViewModel()
        {
            // Design-time
            _projectManager = null!;
            _dialogService = null!;
            Projects = new ObservableCollection<ProjectDashboardItemViewModel>
            {
                new ProjectDashboardItemViewModel { Name = "Construction Schedule", Progress = 7, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2026, 7, 13) },
                new ProjectDashboardItemViewModel { Name = "Engen", Progress = 97, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2025, 11, 6) }
            };
        }

        [RelayCommand]
        private void OpenProject(ProjectDashboardItemViewModel project)
        {
            if (project == null) return;
            WeakReferenceMessenger.Default.Send(new Messages.ProjectSelectedMessage(new Project { Id = project.Id, Name = project.Name }));
        }

        [RelayCommand]
        public async Task LoadProjects()
        {
            if (_projectManager == null) return;

            try 
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Loading Projects...");
                var projects = await _projectManager.GetProjectsAsync();
                
                // Transform to dashboard items
                // This is a mockup transformation since we might not have all this data in the plain Project model yet
                var dashboardItems = projects.Select(p => new ProjectDashboardItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Progress = 0, // Mockup
                    ProjectManagerInitials = "OR", // Mockup
                    Members = new() { "OR" }, // Mockup
                    Status = "Planning", // Mockup
                    LatestFinish = p.EndDate
                });

                Projects = new ObservableCollection<ProjectDashboardItemViewModel>(dashboardItems);
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Load Projects Complete. Count: {Projects.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] CRASH in LoadProjects: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Stack: {ex.StackTrace}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading projects: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        private void NewProject()
        {
            // Logic to create new project (maybe navigate to wizard)
        }
    }
}
