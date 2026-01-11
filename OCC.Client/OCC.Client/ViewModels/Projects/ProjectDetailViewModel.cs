using Avalonia.Threading;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Shared.Models;
using System;
using OCC.Client.Services;

using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;

using System.Collections.Generic;
using System.Linq;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IAuthService _authService;
        private readonly System.Threading.SemaphoreSlim _loadLock = new(1, 1);

        #endregion

        #region Events

        public event EventHandler<Guid>? TaskSelectionRequested;
        public event EventHandler? NewTaskRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _tasks = new();

        [ObservableProperty]
        private Shared.ProjectTopBarViewModel _topBar;

        [ObservableProperty]
        private ProjectTask? _selectedTask;

        [ObservableProperty]
        private Guid _currentProjectId;

        #endregion

        #region Properties

        public bool HasTasks => Tasks.Count > 0;

        #endregion

        #region Constructors

        public ProjectDetailViewModel()
        {
            // Parameterless constructor for design-time support
            _taskRepository = null!;
            _projectRepository = null!;
            _authService = null!;
            _topBar = new Shared.ProjectTopBarViewModel();
        }
        
        public ProjectDetailViewModel(
            IRepository<ProjectTask> taskRepository,
            IRepository<Project> projectRepository,
            IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            _topBar = new Shared.ProjectTopBarViewModel();
            
            // Subscribe to updates
            WeakReferenceMessenger.Default.Register<ViewModels.Messages.TaskUpdatedMessage>(this, (r, m) =>
            {
                if (CurrentProjectId != Guid.Empty)
                {
                    LoadTasks(CurrentProjectId);
                }
            });
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddNewTask()
        {
            if (CurrentProjectId == Guid.Empty) return;
            NewTaskRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public async void LoadTasks(Guid projectId)
        {
            await _loadLock.WaitAsync();
            try
            {
                CurrentProjectId = projectId;
                
                // Load Project Details for TopBar
                var project = await _projectRepository.GetByIdAsync(projectId);
                if (project != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        TopBar.ProjectName = project.Name;
                        TopBar.ProjectId = project.Id;
                        TopBar.ProjectIconInitials = GetInitials(project.Name);
                        // TopBar.TrialDaysLeft? 
                    });
                }

                var tasks = await _taskRepository.FindAsync(t => t.ProjectId == projectId);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Tasks.Clear();
                    var flatList = BuildTaskHierarchy(tasks);
                    foreach (var task in flatList)
                    {
                        Tasks.Add(task);
                    }
                    OnPropertyChanged(nameof(HasTasks));
                });
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private List<ProjectTask> _rootTasks = new();

        private List<ProjectTask> BuildTaskHierarchy(IEnumerable<ProjectTask> allTasks)
        {
            var taskList = allTasks.OrderBy(t => t.OrderIndex).ToList();
            _rootTasks.Clear();

            // Stack-based hierarchy inference (MSP Style)
            // This allows us to build the tree even if ParentId is not explicitly set in the DB,
            // relying on OrderIndex + IndentLevel.
            var parentStack = new Stack<ProjectTask>();

            foreach (var task in taskList)
            {
                task.Children.Clear(); // Reset children

                // 1. Find correct parent in stack
                // Pop while the potential parent at the top is a sibling or deeper (IndentLevel >= current)
                // We want a parent with IndentLevel < current
                while (parentStack.Count > 0 && parentStack.Peek().IndentLevel >= task.IndentLevel)
                {
                    parentStack.Pop();
                }

                if (parentStack.Count > 0)
                {
                    // Found a parent
                    var parent = parentStack.Peek();
                    parent.Children.Add(task);
                    task.ParentId = parent.Id; // Fix up model in memory if needed
                }
                else
                {
                    // No parent, it's a root
                    _rootTasks.Add(task);
                }

                // Push current as potential parent for next items
                parentStack.Push(task);
            }
            
            // 2. Refresh the display list based on expansion state
            return RefreshTaskList();
        }

        private List<ProjectTask> RefreshTaskList()
        {
            var flatList = new List<ProjectTask>();
            foreach (var rootTask in _rootTasks)
            {
                FlattenTask(rootTask, flatList, 0);
            }
            return flatList;
        }

        private void FlattenTask(ProjectTask task, List<ProjectTask> flatList, int level)
        {
            task.IndentLevel = level;
            flatList.Add(task);

            // Only add children if expanded
            if (task.IsExpanded && task.Children != null && task.Children.Any())
            {
                foreach (var child in task.Children) 
                {
                   // Recursively add children
                   FlattenTask(child, flatList, level + 1);
                }
            }
        }

        [RelayCommand]
        private void ToggleExpand(ProjectTask task)
        {
            if (task == null) return;
            task.IsExpanded = !task.IsExpanded;
            
            // Re-flatten and update UI
            var newFlatList = RefreshTaskList();
            
            Tasks.Clear();
            foreach(var t in newFlatList)
            {
                Tasks.Add(t);
            }
        }
        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "P";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        partial void OnSelectedTaskChanged(ProjectTask? value)
        {
            if (value != null)
            {
                TaskSelectionRequested?.Invoke(this, value.Id);
                SelectedTask = null; // Reset selection so it can be clicked again
            }
        }

        #endregion
    }
}
