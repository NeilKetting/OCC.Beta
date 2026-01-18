using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.ViewModels.Projects.Dashboard;
using OCC.Client.ViewModels.Messages;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectListViewModel : ViewModelBase, 
        IRecipient<TaskUpdatedMessage>, 
        IRecipient<EntityUpdatedMessage>
    {
        private readonly IProjectManager _projectManager;
        private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private ObservableCollection<ProjectDashboardItemViewModel> _projects = new();

        [ObservableProperty]
        private ProjectDashboardItemViewModel? _selectedProject;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private CreateProjectViewModel? _createProjectVM;

        [ObservableProperty]
        private bool _isCreateProjectVisible;

        public ProjectListViewModel()
        {
            // Design-time
            _projectManager = null!;
            _dialogService = null!;
            _serviceProvider = null!;
            IsCreateProjectVisible = false;
            Projects = new ObservableCollection<ProjectDashboardItemViewModel>
            {
                new ProjectDashboardItemViewModel { Name = "Construction Schedule", Progress = 7, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2026, 7, 13) },
                new ProjectDashboardItemViewModel { Name = "Engen", Progress = 97, ProjectManagerInitials = "OR", Members = new() { "OR" }, Status = "Deleted", LatestFinish = new DateTime(2025, 11, 6) }
            };
        }


        public ProjectListViewModel(IProjectManager projectManager, IDialogService dialogService, IServiceProvider serviceProvider)
        {
            _projectManager = projectManager;
            _dialogService = dialogService;
            _serviceProvider = serviceProvider;

            // Register for messages
            WeakReferenceMessenger.Default.RegisterAll(this);
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

            BusyText = "Loading projects...";
            IsBusy = true;
            try 
            {
                System.Diagnostics.Debug.WriteLine($"[ProjectsListViewModel] Loading Projects...");
                var projects = await _projectManager.GetProjectsAsync();
                
                var dashboardItems = new System.Collections.Generic.List<ProjectDashboardItemViewModel>();

                foreach (var p in projects)
                {
                    // Calculate Progress and Latest Finish
                    var taskList = (await _projectManager.GetTasksForProjectAsync(p.Id))?.ToList() ?? new List<ProjectTask>();
                    int progress = 0;
                    DateTime? latestFinish = p.EndDate;
                    
                    if (taskList.Any())
                    {
                        progress = (int)Math.Round(taskList.Average(t => t.PercentComplete));
                        
                        var taskLatest = taskList.Max(t => t.FinishDate);
                        if (taskLatest > DateTime.MinValue)
                        {
                            latestFinish = taskLatest;
                        }
                    }

                    dashboardItems.Add(new ProjectDashboardItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Progress = progress,
                        ProjectManagerInitials = !string.IsNullOrEmpty(p.ProjectManager) ? p.ProjectManager.Substring(0, Math.Min(2, p.ProjectManager.Length)).ToUpper() : "OR",
                        Status = p.Status,
                        LatestFinish = latestFinish
                    });
                }

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void NewProject()
        {
            CreateProjectVM = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<CreateProjectViewModel>(_serviceProvider);
            CreateProjectVM.CloseRequested += (s, e) => IsCreateProjectVisible = false;
            CreateProjectVM.ProjectCreated += (s, id) => 
            {
                IsCreateProjectVisible = false;
                _ = LoadProjects();
            };
            IsCreateProjectVisible = true;
        }

        public void Receive(TaskUpdatedMessage message)
        {
            // Reload to update progress
            // In a more optimized version, we would find the specific project and only re-calculate it.
            _ = LoadProjects();
        }

        public void Receive(EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "ProjectTask" || message.Value.EntityType == "Task")
            {
                _ = LoadProjects();
            }
        }
    }
}
