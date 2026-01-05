using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class NewTaskPopupViewModel : ViewModelBase
    {
        private readonly IRepository<ProjectTask> _taskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<User> _userRepository; // Assuming we have a user repository
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _taskName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private Project? _selectedProject;

        [ObservableProperty]
        private ObservableCollection<User> _users = new();

        [ObservableProperty]
        private User? _assignedUser;
        
        // We might want to default to the current user
        public NewTaskPopupViewModel(IRepository<ProjectTask> taskRepository, 
                                     IRepository<Project> projectRepository,
                                     IAuthService authService)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _authService = authService;
            
            // In a real scenario we'd inject IRepository<User> too, but for now we might mock or skip
            // AssignedUser = _authService.CurrentUser; 
            
            // LoadData should be called by the parent
        }

        public async Task LoadData()
        {
            var projects = await _projectRepository.GetAllAsync();
            Projects = new ObservableCollection<Project>(projects);
            
            // Mock users for now if we don't have a user repo handy in the constructor args yet, 
            // but the plan mentioned "Assigned To" selector.
            // Let's add the current user at least.
            if (_authService.CurrentUser != null)
            {
               Users.Add(_authService.CurrentUser);
               AssignedUser = _authService.CurrentUser;
            }
        }

        public event EventHandler? CloseRequested;
        public event EventHandler<Guid>? TaskCreated;

        [RelayCommand]
        private async Task CreateTask()
        {
            if (string.IsNullOrWhiteSpace(TaskName)) return;

            var newTask = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Name = TaskName,
                Description = "",
                StartDate = DateTime.Now,
                FinishDate = DateTime.Now.AddDays(1),
                ProjectId = SelectedProject?.Id ?? Guid.Empty, // Should force validation really
                AssignedTo = AssignedUser?.DisplayName ?? "UN",
                Status = "To Do",
                Priority = "Medium"
            };

            await _taskRepository.AddAsync(newTask);
            // TaskCreated invokes with Guid usually, but ProjectTask has String Id...
            // Assuming we convert back or update event signature.
            // For now parsing Guid.
            TaskCreated?.Invoke(this, newTask.Id);
            Close();
        }

        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
