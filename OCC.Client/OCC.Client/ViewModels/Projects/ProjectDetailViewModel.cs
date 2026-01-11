using Avalonia.Threading;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Shared.Models;
using System;
using OCC.Client.Services;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IProjectManager _projectManager;
        private readonly System.Threading.SemaphoreSlim _loadLock = new(1, 1);
        private List<ProjectTask> _rootTasks = new();

        #endregion

        #region Events

        public event EventHandler<Guid>? TaskSelectionRequested;
        public event EventHandler? NewTaskRequested;

        #endregion

        #region Observables

        [ObservableProperty]
        private Shared.ProjectTopBarViewModel _topBar;

        [ObservableProperty]
        private Core.ViewModelBase _currentView;

        [ObservableProperty]
        private ProjectTaskListViewModel _listVM;

        [ObservableProperty]
        private ProjectGanttViewModel _ganttVM;

        [ObservableProperty]
        private Guid _currentProjectId;

        [ObservableProperty]
        private string _siteManagerInitials = "UA";

        [ObservableProperty]
        private string _siteManagerName = "Unassigned";

        [ObservableProperty]
        private string _siteManagerColor = "#EF4444"; // Red for UA

        [ObservableProperty]
        private bool _isSiteManagerAssigned;

        [ObservableProperty]
        private ObservableCollection<Employee> _availableSiteManagers = new();

        [ObservableProperty]
        private ObservableCollection<Employee> _filteredSiteManagers = new();

        private string _managerSearchText = string.Empty;
        public string ManagerSearchText
        {
            get => _managerSearchText;
            set
            {
                if (SetProperty(ref _managerSearchText, value))
                {
                    ApplyManagerFilter();
                }
            }
        }

        #endregion

        #region Properties

        public bool HasTasks => ListVM.HasTasks;

        #endregion

        #region Constructors

        public ProjectDetailViewModel()
        {
            _projectManager = null!;
            _topBar = new Shared.ProjectTopBarViewModel();
            _listVM = new ProjectTaskListViewModel();
            _ganttVM = new ProjectGanttViewModel();
            _currentView = _listVM;
        }
        
        public ProjectDetailViewModel(IProjectManager projectManager)
        {
            _projectManager = projectManager;
            _topBar = new Shared.ProjectTopBarViewModel();
            _listVM = new ProjectTaskListViewModel();
            _ganttVM = new ProjectGanttViewModel(_projectManager);

            _currentView = _listVM;

            _listVM.TaskSelectionRequested += (s, id) => TaskSelectionRequested?.Invoke(this, id);
            _listVM.ToggleExpandRequested += (s, e) => { RefreshDisplayList(); };

            _topBar.PropertyChanged += TopBar_PropertyChanged;
            
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

        [RelayCommand]
        private async Task AssignSiteManager(Employee manager)
        {
            if (CurrentProjectId == Guid.Empty || manager == null) return;

            await _projectManager.AssignSiteManagerAsync(CurrentProjectId, manager.Id);
            
            // UI Update
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SiteManagerName = manager.DisplayName;
                SiteManagerInitials = GetInitials(manager.DisplayName);
                SiteManagerColor = "#14B8A6"; 
                IsSiteManagerAssigned = true;
            });
        }

        [RelayCommand]
        private void ToggleExpand(ProjectTask task)
        {
            if (task == null) return;
            _projectManager.ToggleExpand(task);
            RefreshDisplayList();
        }

        #endregion

        #region Methods

        public async void LoadTasks(Guid projectId)
        {
            await _loadLock.WaitAsync();
            try
            {
                CurrentProjectId = projectId;
                
                var project = await _projectManager.GetProjectByIdAsync(projectId);
                var managers = await _projectManager.GetSiteManagersAsync();
                var tasks = await _projectManager.GetTasksForProjectAsync(projectId);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (project != null)
                    {
                        TopBar.ProjectName = project.Name;
                        TopBar.ProjectId = project.Id;
                        TopBar.ProjectIconInitials = GetInitials(project.Name);

                        if (project.SiteManager != null)
                        {
                            SiteManagerName = project.SiteManager.DisplayName;
                            SiteManagerInitials = GetInitials(project.SiteManager.DisplayName);
                            SiteManagerColor = "#14B8A6"; 
                            IsSiteManagerAssigned = true;
                        }
                        else
                        {
                            SiteManagerName = "Unassigned";
                            SiteManagerInitials = "UA";
                            SiteManagerColor = "#EF4444";
                            IsSiteManagerAssigned = false;
                        }
                    }

                    AvailableSiteManagers.Clear();
                    foreach (var m in managers) AvailableSiteManagers.Add(m);
                    ApplyManagerFilter();

                    _rootTasks = _projectManager.BuildTaskHierarchy(tasks);
                    RefreshDisplayList();
                    GanttVM.LoadTasks(projectId);
                });
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private void TopBar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Shared.ProjectTopBarViewModel.ActiveTab))
            {
                switch (TopBar.ActiveTab)
                {
                    case "List":
                        CurrentView = ListVM;
                        break;
                    case "Gantt":
                        CurrentView = GanttVM;
                        break;
                }
            }
        }

        private void RefreshDisplayList()
        {
            var flatList = _projectManager.FlattenHierarchy(_rootTasks);
            ListVM.UpdateTasks(flatList);
            OnPropertyChanged(nameof(HasTasks));
        }

        private void ApplyManagerFilter()
        {
            var filtered = string.IsNullOrWhiteSpace(ManagerSearchText)
                ? AvailableSiteManagers
                : AvailableSiteManagers.Where(m => m.DisplayName.Contains(ManagerSearchText, StringComparison.OrdinalIgnoreCase));

            FilteredSiteManagers.Clear();
            foreach (var m in filtered) FilteredSiteManagers.Add(m);
        }

        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "P";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        #endregion
    }
}
