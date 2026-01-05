using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Dashboard;
using OCC.Client.ViewModels.Home.Tasks;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels.Home.ProjectSummary;
using OCC.Client.ViewModels.Time;
// using OCC.Client.ViewModels.StaffManagement;
using OCC.Client.ViewModels.Projects;
using OCC.Client.ViewModels.Notifications;
using OCC.Client.ViewModels.Shared;
using OCC.Client.ViewModels.Messages;
using OCC.Client.ViewModels.Settings;


namespace OCC.Client.ViewModels.Home
{
    public partial class HomeViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly ITimeService _timeService;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ProjectTask> _projectTaskModelRepository;
        private readonly IRepository<AppSetting> _appSettingsRepository;
        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<TaskAssignment> _taskAssignmentRepository;
        private readonly IRepository<TaskComment> _commentRepository;
        private readonly IRepository<User> _userRepository;

        [ObservableProperty]
        private TopBarViewModel _topBar;

        [ObservableProperty]
        private SummaryViewModel _mySummary;

        [ObservableProperty]
        private TasksWidgetViewModel _myTasks;

        [ObservableProperty]
        private PulseViewModel _projectPulse;

        [ObservableProperty]
        private ProjectSummaryViewModel _projectSummary;

        [ObservableProperty]
        private TeamSummaryViewModel _teamSummary;
        
        // Dashboard Visibility Logic
        [ObservableProperty]
        private bool _isDashboardVisible = true;

        [ObservableProperty]
        private bool _isMySummaryVisible = true;

        [ObservableProperty]
        private bool _isTeamSummaryVisible = false;

        [ObservableProperty]
        private bool _isProjectSummaryVisible = false;

        [ObservableProperty]
        private string _greeting = string.Empty;

        [ObservableProperty]
        private string _currentDate = DateTime.Now.ToString("dd MMMM yyyy");

        public bool IsTopBarVisible => true; // Always visible on Dashboard for now

        public HomeViewModel(TopBarViewModel topBar, 
                             SummaryViewModel mySummary, 
                             TasksWidgetViewModel myTasks, 
                             PulseViewModel projectPulse,
                             ProjectSummaryViewModel projectSummary,
                             IAuthService authService,
                             ITimeService timeService,
                             IRepository<ProjectTask> projectTaskRepository,
                             IRepository<Project> projectRepository,
                             IRepository<Customer> customerRepository,
                             IRepository<ProjectTask> projectTaskModelRepository,
                             IRepository<AppSetting> appSettingsRepository,
                             IRepository<Employee> staffRepository,
                             IRepository<TaskAssignment> taskAssignmentRepository,
                             IRepository<TaskComment> commentRepository,
                              IRepository<User> userRepository)
        {
            _authService = authService;
            _timeService = timeService;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _customerRepository = customerRepository;
            _projectTaskModelRepository = projectTaskModelRepository;
            _appSettingsRepository = appSettingsRepository;
            _staffRepository = staffRepository;
            _taskAssignmentRepository = taskAssignmentRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            TopBar = topBar;
            MySummary = mySummary;
            MyTasks = myTasks;
            ProjectPulse = projectPulse;
            ProjectSummary = projectSummary;
            TeamSummary = new TeamSummaryViewModel();

            WeakReferenceMessenger.Default.Register<CreateProjectMessage>(this, (r, m) => OpenCreateProject());
            WeakReferenceMessenger.Default.Register<CreateNewTaskMessage>(this, (r, m) => OpenNewTaskPopup());

            TopBar.PropertyChanged += TopBar_PropertyChanged;

            Initialize();
        }

        private void Initialize()
        {
            var now = DateTime.Now;
            Greeting = GetGreeting(now);
            CurrentDate = now.ToString("dd MMMM yyyy");
        }

        private void TopBar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TopBarViewModel.ActiveTab))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            // Reset all
            IsMySummaryVisible = false;
            IsTeamSummaryVisible = false;
            IsProjectSummaryVisible = false;

            // Only care about tabs relevant to Dashboard
            switch (TopBar.ActiveTab)
            {
                case "Portfolio Summary": 
                case "Project Summary":
                    IsProjectSummaryVisible = true;
                    break;
                case "Team Summary":
                    IsTeamSummaryVisible = true;
                    break;
                case "My Summary":
                default:
                    IsMySummaryVisible = true;
                    break;
            }
        }

        [ObservableProperty]
        private bool _isTaskDetailVisible = false;

        [ObservableProperty]
        private TaskDetailViewModel? _currentTaskDetail;

        [RelayCommand]
        private void OpenTaskDetail(Guid taskId)
        {
            CurrentTaskDetail = new TaskDetailViewModel(_projectTaskRepository, _staffRepository, _taskAssignmentRepository, _commentRepository);
            CurrentTaskDetail.CloseRequested += (s, e) => CloseTaskDetail();
            CurrentTaskDetail.LoadTaskById(taskId);
            IsTaskDetailVisible = true;
        }

        [RelayCommand]
        private void CloseTaskDetail()
        {
            IsTaskDetailVisible = false;
            CurrentTaskDetail = null;
        }

        private async void CreateNewTask()
        {
            var newTask = new ProjectTask
            {
                Name = "New Task",
                Description = "",
            };

            await _projectTaskRepository.AddAsync(newTask);
            OpenTaskDetail(newTask.Id);
        }

        private string GetGreeting(DateTime time)
        {
            string timeGreeting = time.Hour < 12 ? "Good morning" :
                                  time.Hour < 18 ? "Good afternoon" : "Good evening";
            
            var userName = _authService.CurrentUser?.DisplayName ?? "User";
            return $"{timeGreeting}, {userName}";
        }


        
        [ObservableProperty]
        private bool _isNewTaskPopupVisible = false;

        [ObservableProperty]
        private NewTaskPopupViewModel? _newTaskPopup;

        private void OpenNewTaskPopup()
        {
           NewTaskPopup = new NewTaskPopupViewModel(_projectTaskRepository, _projectRepository, _authService);
           _ = NewTaskPopup.LoadData();
           
           NewTaskPopup.CloseRequested += (s, e) => CloseNewTaskPopup();
           IsNewTaskPopupVisible = true;
        }

        private void CloseNewTaskPopup()
        {
            IsNewTaskPopupVisible = false;
            NewTaskPopup = null;
        }

        [ObservableProperty]
        private bool _isCreateProjectVisible;

        [ObservableProperty]
        private CreateProjectViewModel? _createProjectVM;

        private void OpenCreateProject()
        {
            CreateProjectVM = new CreateProjectViewModel(_projectRepository, _customerRepository, _projectTaskModelRepository, _appSettingsRepository, _staffRepository);
            CreateProjectVM.CloseRequested += (s, e) => CloseCreateProject();
            CreateProjectVM.ProjectCreated += ProjectCreatedHandler;
            IsCreateProjectVisible = true;
        }

        private void CloseCreateProject()
        {
            IsCreateProjectVisible = false;
            CreateProjectVM = null;
        }

        private void ProjectCreatedHandler(object? sender, Guid projectId)
        {
            WeakReferenceMessenger.Default.Send(new ProjectCreatedMessage(new Project { Id = projectId })); // Sidebar listens to this
            // We can't navigate to Projects tab from here anymore as we are decoupled.
            // But we can send a message. Sidebar listens for 'ProjectCreatedMessage' but maybe we need a 'NavigateToProjectMessage'.
             WeakReferenceMessenger.Default.Send(new ProjectSelectedMessage(new Project { Id = projectId })); // Or similar
        }
    }
}
