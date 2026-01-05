using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Shared.Models;
using System;
using OCC.Client.Services;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectListViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ProjectTask> _tasks = new();

        [ObservableProperty]
        private ProjectTask? _selectedTask;

        public event EventHandler<Guid>? TaskSelectionRequested;

        partial void OnSelectedTaskChanged(ProjectTask? value)
        {
            if (value != null)
            {
                TaskSelectionRequested?.Invoke(this, value.Id);
                SelectedTask = null; // Reset selection so it can be clicked again
            }
        }

        public bool HasTasks => Tasks.Count > 0;

        [ObservableProperty]
        private Guid _currentProjectId;



        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;

        public ProjectListViewModel(
            IRepository<ProjectTask> taskRepository,
            IRepository<Project> projectRepository,
            IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            
            // Subscribe to updates
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<ViewModels.Messages.TaskUpdatedMessage>(this, (r, m) =>
            {
                if (CurrentProjectId != Guid.Empty)
                {
                    LoadTasks(CurrentProjectId);
                }
            });
        }

        public async void LoadTasks(Guid projectId)
        {
            CurrentProjectId = projectId;
            var tasks = await _taskRepository.FindAsync(t => t.ProjectId == projectId);
            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }
            OnPropertyChanged(nameof(HasTasks));
        }

        public event EventHandler? NewTaskRequested;

        [RelayCommand]
        private void AddNewTask()
        {
            if (CurrentProjectId == Guid.Empty) return;
            NewTaskRequested?.Invoke(this, EventArgs.Empty);
        }


    }
}
