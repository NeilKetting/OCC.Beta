using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Home.Shared;
using OCC.Client.ViewModels; // Assuming ViewModelBase is here

namespace OCC.Client.ViewModels.Home.Tasks
{
    public partial class TaskListViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<HomeTaskItem> _tasks = new();

        public event EventHandler<string>? TaskSelectionRequested;

        private readonly IRepository<ProjectTask> _taskRepository;

        public TaskListViewModel(IRepository<ProjectTask> taskRepository)
        {
            _taskRepository = taskRepository;
            LoadTasks();

            // Subscribe to updates
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<Messages.TaskUpdatedMessage>(this, (r, m) =>
            {
                // Simple refresh for now. Optimally, find and update the specific item.
                LoadTasks();
            });
        }

        public async void LoadTasks()
        {
            var tasks = await _taskRepository.GetAllAsync();
            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(new HomeTaskItem
                {
                    Id = task.Id, // Converting string Id to Guid for HomeTaskItem if needed, or update HomeTaskItem to string
                    Title = task.Name,
                    Description = task.Description,
                    DueDate = task.FinishDate, // Mapping FinishDate to DueDate
                    Status = task.Status, // Using Status string
                    Priority = task.Priority,
                    AssigneeInitials = task.AssignedTo.Substring(0, Math.Min(2, task.AssignedTo.Length)).ToUpper()
                });
            }
        }

        [RelayCommand]
        private void SelectTask(Guid taskId)
        {
            TaskSelectionRequested?.Invoke(this, taskId.ToString());
        }

        public event EventHandler? NewTaskRequested;

        [RelayCommand]
        public void NewTask()
        {
            NewTaskRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
