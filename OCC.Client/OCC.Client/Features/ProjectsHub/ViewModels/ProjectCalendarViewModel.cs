using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.ProjectsHub.ViewModels
{
    public partial class ProjectCalendarViewModel : ViewModelBase, IRecipient<TaskUpdatedMessage>
    {
        private readonly IHolidayService _holidayService;
        private readonly IProjectManager _projectManager;
        private Guid _projectId;

        [ObservableProperty]
        private DateTime _currentMonth;

        [ObservableProperty]
        private string _monthName = string.Empty;

        [ObservableProperty]
        private string _yearName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProjectCalendarDayViewModel> _days = new();

        [ObservableProperty]
        private ObservableCollection<string> _weekDays = new() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        public ProjectCalendarViewModel(
            IHolidayService holidayService,
            IProjectManager projectManager)
        {
            _holidayService = holidayService;
            _projectManager = projectManager;

            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            WeakReferenceMessenger.Default.Register(this);
        }

        // For Design Time
        public ProjectCalendarViewModel()
        {
            _holidayService = null!;
            _projectManager = null!;
            CurrentMonth = DateTime.Now;
        }

        public void LoadProject(Guid projectId)
        {
            _projectId = projectId;
            GenerateCalendar();
        }

        public void Receive(TaskUpdatedMessage message)
        {
            // If any task in the project is updated, refresh our view
            GenerateCalendar();
        }

        [RelayCommand]
        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
            GenerateCalendar();
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
            GenerateCalendar();
        }

        private async void GenerateCalendar()
        {
            if (_holidayService == null || _projectId == Guid.Empty) return;

            MonthName = CurrentMonth.ToString("MMMM");
            YearName = CurrentMonth.ToString("yyyy");
            
            // 1. Setup Grid
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            
            int offset = (int)firstDayOfMonth.DayOfWeek - 1; 
            if (offset < 0) offset = 6; 

            var previousMonth = CurrentMonth.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            var dayList = new List<ProjectCalendarDayViewModel>();

            // Previous Month padding
            for (int i = 0; i < offset; i++)
            {
                var d = daysInPrevMonth - offset + 1 + i;
                dayList.Add(new ProjectCalendarDayViewModel(new DateTime(previousMonth.Year, previousMonth.Month, d), false));
            }

            // Current Month
            for (int i = 1; i <= daysInMonth; i++)
            {
                dayList.Add(new ProjectCalendarDayViewModel(new DateTime(CurrentMonth.Year, CurrentMonth.Month, i), true));
            }

            // Next Month Padding
            int remaining = 42 - dayList.Count;
            var nextMonth = CurrentMonth.AddMonths(1);
            for (int i = 1; i <= remaining; i++)
            {
                dayList.Add(new ProjectCalendarDayViewModel(new DateTime(nextMonth.Year, nextMonth.Month, i), false));
            }

            // 2. Fetch Data (Holidays)
            var holidays = await _holidayService.GetHolidaysForYearAsync(CurrentMonth.Year);
            
            foreach (var day in dayList)
            {
                var holiday = holidays.FirstOrDefault(h => h.Date.Date == day.Date.Date);
                if (holiday != null)
                {
                    day.IsHoliday = true;
                    day.HolidayName = holiday.Name;
                }
            }

            // 3. Populate Tasks (Async)
            await LoadTaskDataAsync(dayList);

            // Update ObservableCollection
            Days.Clear();
            foreach (var d in dayList) Days.Add(d);
        }

        private async Task LoadTaskDataAsync(List<ProjectCalendarDayViewModel> days)
        {
            if (_projectId == Guid.Empty || !days.Any()) return;

            var start = days.First().Date;
            var end = days.Last().Date;

            var allTasks = await _projectManager.GetTasksForProjectAsync(_projectId);
            var visibleTasks = allTasks.Where(t => 
                (t.FinishDate.Date >= start.Date && t.StartDate.Date <= end.Date)
            ).OrderBy(t => t.StartDate).ThenByDescending(t => (t.FinishDate - t.StartDate).Days).ToList();

            foreach (var task in visibleTasks)
            {
                string color = task.PercentComplete == 100 ? "#10B981" : "#3B82F6"; // Green if done, Blue otherwise
                if (task.IsOnHold) color = "#94A3B8"; // Gray

                foreach (var day in days)
                {
                    if (day.Date >= task.StartDate.Date && day.Date <= task.FinishDate.Date)
                    {
                        var item = new ProjectCalendarItemViewModel
                        {
                            Id = task.Id,
                            Text = task.Name,
                            Tooltip = $"{task.Name} ({task.StartDate:dd/MM} - {task.FinishDate:dd/MM}) - {task.PercentComplete}% Complete",
                            Color = color
                        };

                        bool isStart = day.Date == task.StartDate.Date;
                        bool isEnd = day.Date == task.FinishDate.Date;

                        if (isStart && isEnd) item.Span = ProjectCalendarItemSpan.Single;
                        else if (isStart) item.Span = ProjectCalendarItemSpan.Start;
                        else if (isEnd) item.Span = ProjectCalendarItemSpan.End;
                        else item.Span = ProjectCalendarItemSpan.Middle;

                        day.Items.Add(item);
                    }
                }
            }
        }
    }

    public partial class ProjectCalendarDayViewModel : ObservableObject
    {
        public DateTime Date { get; }
        public int DayNumber => Date.Day;
        public bool IsCurrentMonth { get; }
        public bool IsNotCurrentMonth => !IsCurrentMonth;
        public bool IsToday { get; }
        public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;

        [ObservableProperty]
        private bool _isHoliday;

        [ObservableProperty]
        private string? _holidayName;

        public ObservableCollection<ProjectCalendarItemViewModel> Items { get; } = new();

        public ProjectCalendarDayViewModel(DateTime date, bool isCurrentMonth)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            IsToday = date.Date == DateTime.Today;
        }
    }

    public enum ProjectCalendarItemSpan { Single, Start, Middle, End }

    public class ProjectCalendarItemViewModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public string Color { get; set; } = "#3B82F6"; 
        public ProjectCalendarItemSpan Span { get; set; } = ProjectCalendarItemSpan.Single;
    }
}




