using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using OCC.Client.Services;
using OCC.Shared.Models;
using System.Threading.Tasks;
using System.Threading;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ModelWrappers;

namespace OCC.Client.ViewModels.Projects.Tasks
{
    /// <summary>
    /// ViewModel for displaying and editing details of a specific ProjectTask.
    /// This ViewModel uses the Model Wrapper pattern (<see cref="ProjectTaskWrapper"/>) to 
    /// separate presentation logic from the data model and ensure clean, reactive bindings.
    /// It manages subtasks, assignments, comments, and orchestrates data loading and saving via repositories.
    /// </summary>
    public partial class TaskDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<TaskAssignment> _assignmentRepository;
        private readonly IRepository<TaskComment> _commentRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        

        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);
        private bool _hasPendingUpdate = false;
        private System.Threading.CancellationTokenSource? _debounceCts;
        private Guid _currentTaskId;

        [ObservableProperty]
        private ProjectTaskWrapper _task;

        // ...



        [ObservableProperty]
        private string _newCommentContent = string.Empty;

        [ObservableProperty]
        private string _newToDoContent = string.Empty;

        [ObservableProperty]
        private bool _isShowingAllSubtasks;

        [ObservableProperty]
        private bool _isCreateMode;

        [ObservableProperty]
        private Project? _selectedProject;

        public ObservableCollection<Project> AvailableProjects { get; } = new();

        [ObservableProperty]
        private bool _hasMoreSubtasks;



        public ObservableCollection<TaskComment> Comments { get; } = new();
        public ObservableCollection<ProjectTask> Subtasks { get; } = new();
        public ObservableCollection<ProjectTask> VisibleSubtasks { get; } = new();
        public ObservableCollection<TaskAssignment> Assignments { get; } = new();
        public ObservableCollection<Employee> AvailableStaff { get; } = new();
        public ObservableCollection<Team> AvailableTeams { get; } = new();
        public ObservableCollection<User> AvailableContractors { get; } = new();
        public ObservableCollection<ModelWrappers.ToDoItemWrapper> ToDoList { get; } = new();

        public int CommentsCount => Comments.Count;
        public int SubtaskCount => Subtasks.Count;
        public int AttachmentsCount => 0; // Placeholder for now

        public event EventHandler? CloseRequested;

        public TaskDetailViewModel(
            IRepository<ProjectTask> projectTaskRepository,
            IRepository<Employee> staffRepository,
            IRepository<Team> teamRepository,
            IRepository<User> userRepository,
            IRepository<Project> projectRepository,
            IRepository<TaskAssignment> assignmentRepository,
            IRepository<TaskComment> commentRepository,
            IDialogService dialogService,
            IAuthService authService)
        {
            _projectTaskRepository = projectTaskRepository;
            _staffRepository = staffRepository;
            _teamRepository = teamRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _assignmentRepository = assignmentRepository;
            _commentRepository = commentRepository;
            _dialogService = dialogService;
            _authService = authService;
            _task = new ProjectTaskWrapper(new ProjectTask());
        }

        // ...

        [RelayCommand]
        private async Task AddComment()
        {
            if (!string.IsNullOrWhiteSpace(NewCommentContent))
            {
                try
                {
                    BusyText = "Posting comment...";
                    IsBusy = true;
                    
                    var user = _authService.CurrentUser;
                    var authorName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";
                    var authorEmail = user?.Email ?? "Unknown";

                    var newComment = new TaskComment
                    {
                        AuthorName = authorName,
                        AuthorEmail = authorEmail,
                        Content = NewCommentContent,
                        CreatedAt = DateTime.Now
                    };
// ...

                Comments.Insert(0, newComment);
                NewCommentContent = string.Empty;
                OnPropertyChanged(nameof(CommentsCount));

                // Save to Task
                await SaveCommentToTask(newComment);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Closes the Task Detail view.
        /// Raises the CloseRequested event and cleans up subscriptions.
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            // Clean up subscription
            if(Task != null) Task.PropertyChanged -= Task_PropertyChanged;
        }

        /// <summary>
        /// Expands the subtask list to show all children, instead of just the preview limit.
        /// </summary>
        [RelayCommand]
        private void ShowAllSubtasks()
        {
            IsShowingAllSubtasks = true;
            UpdateVisibleSubtasks();
        }

        /// <summary>
        /// Navigates to or loads the details of a specific subtask.
        /// </summary>
        /// <param name="subtask">The subtask to open.</param>
        [RelayCommand]
        private void OpenSubtask(ProjectTask subtask)
        {
             if (subtask != null)
             {
                 LoadTaskModel(subtask);
             }
        }

        /// <summary>
        /// Creates a new child task (subtask) under the current task.
        /// Note: This currently creates an in-memory subtask. 
        /// In a full implementation, this should persist the new subtask to the repository.
        /// </summary>
        [RelayCommand]
        private void AddSubtask()
        {
             var newSubtask = new ProjectTask
             {
                 Name = "New Subtask",
                 ProjectId = Guid.Empty, 
                 IndentLevel = 1 
             };
             
             Subtasks.Add(newSubtask);
             UpdateVisibleSubtasks();
        }

        [RelayCommand]
        private void SetStatus(string status)
        {
            if (Task != null)
            {
                Task.Status = status;
            }
        }

        [RelayCommand]
        private void SetPriority(string priority)
        {
            if (Task != null)
            {
                Task.Priority = priority;
            }
        }

        [RelayCommand]
        private void ToggleOnHold()
        {
             // Toggle logic if needed, or set status to On Hold
             if (Task != null)
             {
                 if (Task.Status == "On Hold")
                     Task.Status = "Not Started"; // Or revert to previous? Simplification: Toggle to Not Started
                 else
                     Task.Status = "On Hold";
             }
        }

        [RelayCommand]
        private async Task CommitDurations()
        {
            await UpdateTask();
        }

        [RelayCommand]
        private async Task AssignStaff(Employee staff)
        {
             if (staff == null) return;
             if (Assignments.Any(a => a.AssigneeId == staff.Id && a.AssigneeType == AssigneeType.Staff)) return;

             var assignment = new TaskAssignment
             {
                 TaskId = _currentTaskId,
                 AssigneeId = staff.Id,
                 AssigneeName = staff.DisplayName,
                 AssigneeType = AssigneeType.Staff
             };
             
             Assignments.Add(assignment);
             await SaveAssignment(assignment);
        }

        [RelayCommand]
        private async Task AssignTeam(Team team)
        {
             if (team == null) return;
             if (Assignments.Any(a => a.AssigneeId == team.Id && a.AssigneeType == AssigneeType.Team)) return;

             var assignment = new TaskAssignment
             {
                 TaskId = _currentTaskId,
                 AssigneeId = team.Id,
                 AssigneeName = team.Name,
                 AssigneeType = AssigneeType.Team
             };
             
             Assignments.Add(assignment);
             await SaveAssignment(assignment);
        }

        [RelayCommand]
        private async Task AssignContractor(User contractor)
        {
             if (contractor == null) return;
             if (Assignments.Any(a => a.AssigneeId == contractor.Id && a.AssigneeType == AssigneeType.Contractor)) return;

             var assignment = new TaskAssignment
             {
                 TaskId = _currentTaskId,
                 AssigneeId = contractor.Id,
                 AssigneeName = contractor.DisplayName ?? contractor.Email,
                 AssigneeType = AssigneeType.Contractor
             };
             
             Assignments.Add(assignment);
             await SaveAssignment(assignment);
        }

        private async Task SaveAssignment(TaskAssignment assignment)
        {
             await _updateLock.WaitAsync();
             try 
             {
                await _assignmentRepository.AddAsync(assignment);
             }
             finally
             {
                _updateLock.Release();
             }
        }

        [RelayCommand]
        private void AddToDo()
        {
            if (!string.IsNullOrWhiteSpace(NewToDoContent))
            {
                ToDoList.Add(new ModelWrappers.ToDoItemWrapper(NewToDoContent));
                NewToDoContent = string.Empty;
            }
        }

        [RelayCommand]
        private async Task CreateTask()
        {
            if (Task == null || string.IsNullOrWhiteSpace(Task.Name))
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Task name is required.");
                return;
            }

            if (SelectedProject == null)
            {
                await _dialogService.ShowAlertAsync("Validation Error", "Please select a project.");
                return;
            }

            try 
            {
                BusyText = "Creating task...";
                IsBusy = true;

                Task.CommitToModel();
                var newTask = Task.Model;
                newTask.ProjectId = SelectedProject.Id;

                if (newTask.Id == Guid.Empty) newTask.Id = Guid.NewGuid();
                
                // Set initial properties if not set
                if (string.IsNullOrEmpty(newTask.Status)) newTask.Status = "To Do";
                if (string.IsNullOrEmpty(newTask.Priority)) newTask.Priority = "Medium";
                
                // Date handling
                if (newTask.StartDate == DateTime.MinValue) newTask.StartDate = DateTime.UtcNow;
                if (newTask.FinishDate == DateTime.MinValue) newTask.FinishDate = DateTime.UtcNow.AddDays(1);

                // Add assignments
                foreach (var assign in Assignments)
                {
                    assign.TaskId = newTask.Id;
                }
                newTask.Assignments = Assignments.ToList();

                await _projectTaskRepository.AddAsync(newTask);

                // Notify tree to refresh
                WeakReferenceMessenger.Default.Send(new Messages.TaskUpdatedMessage(newTask.Id));

                // Switch to edit mode or close
                IsCreateMode = false;
                _currentTaskId = newTask.Id;
                
                // Reload to establish proper wrapper tracking and refresh from DB
                LoadTaskById(_currentTaskId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to create task: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads a task by its ID from the repository and initializes all related resources.
        /// </summary>
        /// <param name="taskId">The GUID of the task to load.</param>
        public async void LoadTaskById(Guid taskId)
        {
            try 
            {
                BusyText = "Loading task details...";
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Loading Task ID: {taskId}...");
                
                var task = await _projectTaskRepository.GetByIdAsync(taskId);
                if (task != null) 
                {
                    LoadTaskModel(task);
                }
                else
                {
                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] CRASH in LoadTaskById: {ex.Message}");
                if (_dialogService != null)
                {
                    await _dialogService.ShowAlertAsync("Error", $"Critical Error loading task details: {ex.Message}");
                }
                IsBusy = false;
            }
        }

        /// <summary>
        /// Initializes the ViewModel for creating a new task.
        /// </summary>
        public async void InitializeForCreation(Guid? projectId = null, Guid? parentTaskId = null)
        {
            try 
            {
                IsBusy = true;
                BusyText = "Preparing new task...";
                IsCreateMode = true;

                // 1. Fresh Task Model
                var model = new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId ?? Guid.Empty,
                    ParentId = parentTaskId,
                    Status = "To Do",
                    Priority = "Medium",
                    StartDate = DateTime.UtcNow,
                    FinishDate = DateTime.UtcNow.AddDays(1)
                };

                // 2. Wrap and Setup
                LoadTask(model);
                _currentTaskId = model.Id;

                // 3. Load Lookups
                var projectsTask = _projectRepository.GetAllAsync();
                var staffTask = _staffRepository.GetAllAsync();
                var teamsTask = _teamRepository.GetAllAsync();
                var usersTask = _userRepository.GetAllAsync();

                await System.Threading.Tasks.Task.WhenAll(projectsTask, staffTask, teamsTask, usersTask);

                AvailableProjects.Clear();
                foreach (var p in projectsTask.Result.OrderBy(x => x.Name)) AvailableProjects.Add(p);

                if (projectId.HasValue)
                {
                    SelectedProject = AvailableProjects.FirstOrDefault(p => p.Id == projectId.Value);
                }

                AvailableStaff.Clear();
                foreach (var s in staffTask.Result.Where(e => e.Status == EmployeeStatus.Active)) AvailableStaff.Add(s);

                AvailableTeams.Clear();
                foreach (var t in teamsTask.Result) AvailableTeams.Add(t);

                AvailableContractors.Clear();
                foreach (var u in usersTask.Result.Where(user => user.UserRole == UserRole.ExternalContractor)) AvailableContractors.Add(u);

                // 4. Default Assignment (Self)
                if (_authService.CurrentUser != null)
                {
                    var currentUser = _authService.CurrentUser;
                    var employee = staffTask.Result.FirstOrDefault(e => e.LinkedUserId == currentUser.Id);
                    
                    if (employee != null)
                    {
                        Assignments.Add(new TaskAssignment
                        {
                            TaskId = _currentTaskId,
                            AssigneeId = employee.Id,
                            AssigneeName = employee.DisplayName,
                            AssigneeType = AssigneeType.Staff
                        });
                    }
                    else if (currentUser.UserRole == UserRole.ExternalContractor)
                    {
                        Assignments.Add(new TaskAssignment
                        {
                            TaskId = _currentTaskId,
                            AssigneeId = currentUser.Id,
                            AssigneeName = currentUser.DisplayName ?? currentUser.Email,
                            AssigneeType = AssigneeType.Contractor
                        });
                    }
                }

                // Clear subtasks and comments for new record
                Subtasks.Clear();
                VisibleSubtasks.Clear();
                Comments.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] InitializeForCreation Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Loads a specific task model directly, preserving any child hierarchies already built.
        /// </summary>
        /// <param name="task">The pre-populated ProjectTask model.</param>
        public async void LoadTaskModel(ProjectTask task)
        {
            if (task == null) return;

            try 
            {

                BusyText = "Initializing task details...";
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Loading Task Model: {task.Name} ({task.Id})...");
                
                // Sync fix: We reload the task from DB to ensure we are working on a 
                // separate instance from the one in the List View. This prevents live-updates 
                // in the background list which disappear if we don't save, 
                // and avoids concurrency issues when saving children.
                var freshTask = await _projectTaskRepository.GetByIdAsync(task.Id);
                if (freshTask != null)
                {
                    _currentTaskId = freshTask.Id;
                    LoadTask(freshTask);
                }
                else
                {
                    // Fallback to passed task if DB fetch fails (new task case)
                    _currentTaskId = task.Id;
                    LoadTask(task);
                }
                
                await LoadAssignableResources();
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] LoadTaskModel Complete.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Error in LoadTaskModel: {ex.Message}");
            }
            finally
            {
                IsBusy = false;

            }
        }

        /// <summary>
        /// Loads available staff, comments, and current assignments for the task.
        /// </summary>
        private async Task LoadAssignableResources()
        {
            try 
            {
                var staffTask = _staffRepository.GetAllAsync();
                var teamsTask = _teamRepository.GetAllAsync();
                var usersTask = _userRepository.GetAllAsync();

                await System.Threading.Tasks.Task.WhenAll(staffTask, teamsTask, usersTask);

                AvailableStaff.Clear();
                foreach(var s in staffTask.Result.Where(e => e.Status == EmployeeStatus.Active)) AvailableStaff.Add(s);

                AvailableTeams.Clear();
                foreach(var t in teamsTask.Result) AvailableTeams.Add(t);

                AvailableContractors.Clear();
                foreach(var u in usersTask.Result.Where(user => user.UserRole == UserRole.ExternalContractor)) AvailableContractors.Add(u);
                
                await LoadComments();
                await LoadAssignments();
            }
            catch (Exception ex)
            {
                // Catch secondary load errors
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Error loading resources: {ex.Message}");
                throw; // propagate to parent catch
            }
        }

        /// <summary>
        /// Fetches comments associated with the current task and populates the Comments collection.
        /// </summary>
        private async Task LoadComments()
        {
             Comments.Clear();
             var comments = await _commentRepository.FindAsync(c => c.TaskId == _currentTaskId);
             
             foreach (var comment in comments.OrderByDescending(c => c.CreatedAt))
             {
                 Comments.Add(comment);
             }
             OnPropertyChanged(nameof(CommentsCount));
        }

        /// <summary>
        /// Fetches assignments for the current task and populates the Assignments collection.
        /// </summary>
        private async Task LoadAssignments()
        {
             Assignments.Clear();
              var assignments = await _assignmentRepository.FindAsync(a => a.TaskId == _currentTaskId);
             foreach(var assign in assignments)
             {
                 Assignments.Add(assign);
             }
        }

        /// <summary>
        /// Initializes the ViewModel with a specific ProjectTask instance.
        /// Sets up the wrapper, subscriptions, and subtask visibility.
        /// </summary>
        /// <param name="task">The ProjectTask model to display.</param>
        private void LoadTask(ProjectTask task)
        {
            // Unsubscribe previous
            if (Task != null) Task.PropertyChanged -= Task_PropertyChanged;

            Task = new ProjectTaskWrapper(task);
            Task.PropertyChanged += Task_PropertyChanged;

            // Load Subtasks
            Subtasks.Clear();
            if (task.Children != null)
            {
                foreach(var child in task.Children) Subtasks.Add(child);
            }
            UpdateVisibleSubtasks();

            OnPropertyChanged(nameof(SubtaskCount));
        }

        /// <summary>
        /// Event handler for property changes on the task wrapper.
        /// Triggers an async update to the data model.
        /// </summary>
        private async void Task_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Debounce the update call to avoid spamming the server during typing or rapid clicks
            _debounceCts?.Cancel();
            _debounceCts = new System.Threading.CancellationTokenSource();
            
            try 
            {
                await System.Threading.Tasks.Task.Delay(300, _debounceCts.Token);
                await UpdateTask();
            }
            catch (TaskCanceledException) { }
        }

        /// <summary>
        /// Persists changes from the wrapper back to the data model/database.
        /// Uses a semaphore to ensure serial access and sends a TaskUpdatedMessage upon success.
        /// </summary>
        public async System.Threading.Tasks.Task UpdateTask()
        {
            if (IsCreateMode) return;
            if (_currentTaskId == Guid.Empty) return;

            // If already busy, mark that we need another update once finished
            if (IsBusy)
            {
                _hasPendingUpdate = true;
                return;
            }

            await _updateLock.WaitAsync();
            try
            {
                do
                {
                    _hasPendingUpdate = false;
                    BusyText = "Saving task changes...";
                    IsBusy = true;
                    
                    // Sync Wrapper back to Model
                    Task.CommitToModel();

                // Clean Update: Create a fresh object to send only necessary scalars.
                // This avoids any issues with navigation properties, circular refs, or EF Core tracking.
                var cleanModel = new ProjectTask
                {
                    Id = Task.Model.Id,
                    ProjectId = Task.Model.ProjectId,
                    LegacyId = Task.Model.LegacyId,
                    Name = Task.Model.Name,
                    Description = Task.Model.Description,
                    
                    Status = Task.Model.Status,
                    Priority = Task.Model.Priority,
                    IsOnHold = Task.Model.IsOnHold,
                    PercentComplete = Task.Model.PercentComplete,
                    Type = Task.Model.Type,
                    
                    StartDate = Task.Model.StartDate,
                    FinishDate = Task.Model.FinishDate,
                    Duration = Task.Model.Duration, // Copy string duration
                    
                    ActualStartDate = Task.Model.ActualStartDate,
                    ActualCompleteDate = Task.Model.ActualCompleteDate,
                    PlannedDurationHours = Task.Model.PlannedDurationHours,
                    ActualDuration = Task.Model.ActualDuration,
                    
                    // Structural Properties (Critical for Tree)
                    ParentId = Task.Model.ParentId,
                    OrderIndex = Task.Model.OrderIndex,
                    IndentLevel = Task.Model.IndentLevel,
                    IsGroup = Task.Model.IsGroup,
                    Predecessors = Task.Model.Predecessors ?? new List<string>(),

                    // Set to NULL to act as "Do Not Update" for navigation properties
                    // Now safe as the Shared Model allows nulls for these collections.
                    Children = null!,
                    Assignments = null!,
                    Comments = null!,
                    Project = null!
                };

                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Sending Update for {cleanModel.Id} (Project: {cleanModel.ProjectId}) Priority: {cleanModel.Priority}");

                
                    // Save to DB
                    await _projectTaskRepository.UpdateAsync(cleanModel);
                    
                    // Notify listeners
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new OCC.Client.ViewModels.Messages.TaskUpdatedMessage(_currentTaskId));
                } while (_hasPendingUpdate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TaskDetailViewModel] Update failed: {ex.Message}");
                await _dialogService.ShowAlertAsync("Update Failed", $"Could not save changes: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Saves a new comment to the repository.
        /// </summary>
        /// <param name="comment">The TaskComment to save.</param>
        private async Task SaveCommentToTask(TaskComment comment)
        {
            if (_currentTaskId == Guid.Empty) return;

            await _updateLock.WaitAsync();
            try
            {
                comment.TaskId = _currentTaskId;
                await _commentRepository.AddAsync(comment);
            }
            finally
            {
                _updateLock.Release();
            }
        }

        /// <summary>
        /// Updates the VisibleSubtasks collection based on the 'Show All' toggle and preview limit.
        /// </summary>
        private void UpdateVisibleSubtasks()
        {
            VisibleSubtasks.Clear();
            foreach(var s in Subtasks) VisibleSubtasks.Add(s);
            HasMoreSubtasks = false;
        }

        #endregion
    }
}
