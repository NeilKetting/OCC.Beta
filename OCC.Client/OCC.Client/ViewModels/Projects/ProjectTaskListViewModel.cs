using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Projects
{
    public partial class ProjectTaskListViewModel : Core.ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ProjectTask> _tasks = new();

        [ObservableProperty]
        private ProjectTask? _selectedTask;

        public event EventHandler<Guid>? TaskSelectionRequested;
        public event EventHandler? ToggleExpandRequested;

        public bool HasTasks => Tasks.Count > 0;

        [RelayCommand]
        private void ToggleExpand(ProjectTask task)
        {
            ToggleExpandRequested?.Invoke(this, EventArgs.Empty);
        }

        partial void OnSelectedTaskChanged(ProjectTask? value)
        {
            if (value != null)
            {
                TaskSelectionRequested?.Invoke(this, value.Id);
                SelectedTask = null;
            }
        }

        public void UpdateTasks(IEnumerable<ProjectTask> tasks)
        {
            Tasks.Clear();
            foreach (var task in tasks)
            {
                Tasks.Add(task);
            }
            OnPropertyChanged(nameof(HasTasks));
        }
    }
}
