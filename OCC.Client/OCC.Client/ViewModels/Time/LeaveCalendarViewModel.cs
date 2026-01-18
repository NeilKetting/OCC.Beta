using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class LeaveCalendarViewModel : ViewModelBase
    {
        private readonly IHolidayService _holidayService;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly ILeaveService _leaveService;

        [ObservableProperty]
        private DateTime _currentMonth;

        [ObservableProperty]
        private string _monthName = string.Empty;

        [ObservableProperty]
        private string _yearName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<LeaveCalendarDayViewModel> _days = new();

        [ObservableProperty]
        private ObservableCollection<string> _weekDays = new() { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        public LeaveCalendarViewModel(
            IHolidayService holidayService,
            IRepository<Employee> employeeRepository,
            ILeaveService leaveService)
        {
            _holidayService = holidayService;
            _employeeRepository = employeeRepository;
            _leaveService = leaveService;

            CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            GenerateCalendar();
        }

        // For Design Time
        public LeaveCalendarViewModel()
        {
            _holidayService = null!;
            _employeeRepository = null!;
            _leaveService = null!;
            CurrentMonth = DateTime.Now;
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
            if (_holidayService == null) return;

            MonthName = CurrentMonth.ToString("MMMM");
            YearName = CurrentMonth.ToString("yyyy");
            Days.Clear();

            // 1. Fetch Data
            var holidays = await _holidayService.GetHolidaysForYearAsync(CurrentMonth.Year);
            var employees = await _employeeRepository.GetAllAsync();
            var activeEmployees = employees.Where(e => e.Status == EmployeeStatus.Active).ToList();
            
            // 2. Setup Grid
            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month);
            
            int offset = (int)firstDayOfMonth.DayOfWeek - 1; 
            if (offset < 0) offset = 6; 

            var previousMonth = CurrentMonth.AddMonths(-1);
            var daysInPrevMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            var dayList = new System.Collections.Generic.List<LeaveCalendarDayViewModel>();

            // Previous Month
            for (int i = 0; i < offset; i++)
            {
                var d = daysInPrevMonth - offset + 1 + i;
                dayList.Add(new LeaveCalendarDayViewModel(new DateTime(previousMonth.Year, previousMonth.Month, d), false));
            }

            // Current Month
            for (int i = 1; i <= daysInMonth; i++)
            {
                dayList.Add(new LeaveCalendarDayViewModel(new DateTime(CurrentMonth.Year, CurrentMonth.Month, i), true));
            }

            // Next Month Padding
            int remaining = 42 - dayList.Count;
            var nextMonth = CurrentMonth.AddMonths(1);
            for (int i = 1; i <= remaining; i++)
            {
                dayList.Add(new LeaveCalendarDayViewModel(new DateTime(nextMonth.Year, nextMonth.Month, i), false));
            }

            // 3. Populate Events (Holidays & Birthdays)
            foreach (var day in dayList)
            {
                // Holidays
                var holiday = holidays.FirstOrDefault(h => h.Date.Date == day.Date.Date);
                if (holiday != null)
                {
                    day.IsHoliday = true;
                    day.HolidayName = holiday.Name;
                }

                // Birthdays
                var birthdayBoys = activeEmployees.Where(e => e.DoB.Month == day.Date.Month && 
                                                         e.DoB.Day == day.Date.Day).ToList();
                foreach (var boy in birthdayBoys)
                {
                    day.Items.Add(new CalendarItemViewModel 
                    { 
                        Type = CalendarItemType.Birthday, 
                        Text = $"{boy.FirstName} {boy.LastName}", 
                        Tooltip = $"{boy.DisplayName}'s Birthday! 🎂"
                    });
                }
            }

            // 4. Populate Leave (Async)
            await LoadLeaveDataAsync(dayList, activeEmployees);

            // Add to Observable
            foreach (var d in dayList) Days.Add(d);
        }

        private async Task LoadLeaveDataAsync(System.Collections.Generic.List<LeaveCalendarDayViewModel> days, System.Collections.Generic.List<Employee> employees)
        {
            if (!days.Any()) return;
            var start = days.First().Date;
            var end = days.Last().Date;

            var allRequests = new System.Collections.Generic.List<LeaveRequest>();
            foreach(var emp in employees)
            {
                var reqs = await _leaveService.GetEmployeeRequestsAsync(emp.Id);
                allRequests.AddRange(reqs.Where(r => r.Status == LeaveStatus.Approved && r.EndDate >= start && r.StartDate <= end));
            }

            // Sort logic similar to Gantt/Project Calendar for stacking
             var sortedRequests = allRequests
                .OrderBy(t => t.StartDate)
                .ThenByDescending(t => (t.EndDate - t.StartDate).Days)
                .ToList();

             foreach(var leave in sortedRequests)
             {
                 var emp = employees.FirstOrDefault(e => e.Id == leave.EmployeeId);
                 string name = emp?.DisplayName ?? "Unknown";
                 string color = "#10B981"; // Green for Leave

                 foreach(var day in days)
                 {
                     if (day.Date >= leave.StartDate.Date && day.Date <= leave.EndDate.Date)
                     {
                         var item = new CalendarItemViewModel
                         {
                             Type = CalendarItemType.Leave,
                             Text = name,
                             Tooltip = $"{name} - {leave.LeaveType} ({leave.StartDate:dd/MM} - {leave.EndDate:dd/MM})",
                             Color = color
                         };

                         bool isStart = day.Date == leave.StartDate.Date;
                         bool isEnd = day.Date == leave.EndDate.Date;

                         if (isStart && isEnd) item.Span = CalendarItemSpan.Single;
                         else if (isStart) item.Span = CalendarItemSpan.Start;
                         else if (isEnd) item.Span = CalendarItemSpan.End;
                         else item.Span = CalendarItemSpan.Middle;

                         day.Items.Add(item);
                     }
                 }
             }
        }
    }

    public partial class LeaveCalendarDayViewModel : ObservableObject
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

        public ObservableCollection<CalendarItemViewModel> Items { get; } = new();

        public LeaveCalendarDayViewModel(DateTime date, bool isCurrentMonth)
        {
            Date = date;
            IsCurrentMonth = isCurrentMonth;
            IsToday = date.Date == DateTime.Today;
        }
    }

    public enum CalendarItemType { Birthday, Leave }
    public enum CalendarItemSpan { Single, Start, Middle, End }

    public class CalendarItemViewModel
    {
        public CalendarItemType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public string Color { get; set; } = "#3B82F6"; 
        public CalendarItemSpan Span { get; set; } = CalendarItemSpan.Single;
    }
}
