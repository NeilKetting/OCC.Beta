using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using System;

namespace OCC.Client.ViewModels.Time
{
    public partial class HistoryRecordViewModel : ObservableObject
    {
        private readonly IHolidayService _holidayService;
        private readonly AttendanceRecord _attendance;
        private readonly Employee _employee;
        private bool _isPublicHoliday = false;

        public HistoryRecordViewModel(AttendanceRecord attendance, Employee employee, IHolidayService holidayService)
        {
            _attendance = attendance;
            _employee = employee;
            _holidayService = holidayService;
            
            // Async check for holiday status
            CheckHolidayStatus();
        }

        private async void CheckHolidayStatus()
        {
            if (_attendance.CheckInTime.HasValue || _attendance.ClockInTime.HasValue)
            {
                var d = _attendance.CheckInTime.HasValue ? _attendance.CheckInTime.Value.Date : _attendance.Date;
                _isPublicHoliday = await _holidayService.IsHolidayAsync(d);
                if (_isPublicHoliday)
                {
                    Refresh(); // Trigger data update if holiday status confirmed
                }
            }
        }

        public DateTime Date => _attendance.Date;
        public string EmployeeName => _employee.DisplayName;
        public string Branch => _attendance.Branch; // Or employee branch? Attendance branch preserves history.
        public string PayType => _employee.RateType == RateType.Hourly ? "Hourly" : "Salary";
        
        // Times
        public string InTime => _attendance.CheckInTime?.ToString("HH:mm") ?? _attendance.ClockInTime?.ToString(@"hh\:mm") ?? "--:--";
        public string OutTime => _attendance.CheckOutTime?.ToString("HH:mm") ?? "--:--";
        public string Status => _attendance.Status.ToString();

        // Calculations
        public double HoursWorked
        {
            get
            {
                var checkOut = _attendance.CheckOutTime;
                
                // Live calculation for active sessions (regardless of start date)
                if (!checkOut.HasValue)
                {
                    checkOut = DateTime.Now;
                }

                if (_attendance.CheckInTime.HasValue)
                {
                    if (checkOut.HasValue)
                        return (checkOut.Value - _attendance.CheckInTime.Value).TotalHours;
                }
                
                // Fallback for manual ClockInTime + CheckOutTime/Now
                if (_attendance.ClockInTime.HasValue)
                {
                    var inDt = _attendance.Date.Add(_attendance.ClockInTime.Value);
                    if (checkOut.HasValue)
                        return (checkOut.Value - inDt).TotalHours;
                }
                return 0;
            }
        }
        
        public string HoursWorkedDisplay => HoursWorked > 0 ? $"{HoursWorked:F2}" : "-";

        public decimal Wage
        {
            get
            {
                if (_employee.RateType == RateType.Hourly)
                {
                    // return (decimal)(HoursWorked * _employee.HourlyRate);
                    // NEW: Detailed Calculation (Bucket Logic)
                    return CalculateAccurateWage();
                }
                // RateType.MonthlySalary
                // Refined Logic v2: Dynamic Shift Hours
                // 1. Calculate Daily Rate = Monthly / 21.67
                // 2. Calculate Standard Daily Hours = ShiftEnd - ShiftStart
                // 3. Hourly Rate = Daily Rate / Standard Hours
                // 4. Wage = Hourly Rate * HoursWorked
                if (HoursWorked > 0)
                {
                    decimal monthlyRate = _attendance.CachedHourlyRate ?? (decimal)_employee.HourlyRate;
                    decimal dailyRate = monthlyRate / 21.67m;
                    
                    // Default to 9 hours if shift times are missing, but User says they should be there.
                    double standardHours = 9.0;
                    if (_employee.ShiftStartTime.HasValue && _employee.ShiftEndTime.HasValue)
                    {
                        var span = _employee.ShiftEndTime.Value - _employee.ShiftStartTime.Value;
                        if (span.TotalHours > 0) standardHours = span.TotalHours;
                    }

                    decimal hourlyRate = dailyRate / (decimal)standardHours;
                    return hourlyRate * (decimal)HoursWorked;
                }
                return 0;
            }
        }
        
        private decimal CalculateAccurateWage()
        {
             // 1. Get Start and End Times
             DateTime start;
             if (_attendance.CheckInTime.HasValue) start = _attendance.CheckInTime.Value;
             else if (_attendance.ClockInTime.HasValue) start = _attendance.Date.Add(_attendance.ClockInTime.Value);
             else return 0;

             DateTime end;
             if (_attendance.CheckOutTime.HasValue) end = _attendance.CheckOutTime.Value;
             else end = DateTime.Now; // Live calculation currently

             if (start >= end) return 0;

             decimal totalWage = 0;
             // Fix: Cast double to decimal for the fallback so ?? works
             decimal rateToUse = _attendance.CachedHourlyRate ?? (decimal)_employee.HourlyRate;
             double hourlyRate = (double)rateToUse;
             string branch = _attendance.Branch ?? _employee.Branch ?? "Johannesburg";
             
             // 2. Iterate through time in small chunks (e.g. 15 mins) or analyze spans.
             // For precision and handling "cross-midnight" easily, let's step through.
             // Optimization: Step by 30 mins or calculate span intersections.
             // Given the requirements, a span intersect approach is better but complex to write inline.
             // Let's use a "Chunking" loop (15 min intervals).
             
             var current = start;
             var interval = TimeSpan.FromMinutes(15);
             
             while (current < end)
             {
                 var next = current.Add(interval);
                 if (next > end) next = end;
                 
                 var durationHours = (next - current).TotalHours;
                 var multiplier = GetMultiplier(current, branch);
                 
                 totalWage += (decimal)(durationHours * hourlyRate * multiplier);
                 
                 current = next;
             }
             
             return totalWage;
        }

        public double OvertimeHours
        {
            get
            {
                // Similar to Wage Calculation but summing hours where multiplier > 1.0
                 // 1. Get Start and End Times
                 DateTime start;
                 if (_attendance.CheckInTime.HasValue) start = _attendance.CheckInTime.Value;
                 else if (_attendance.ClockInTime.HasValue) start = _attendance.Date.Add(_attendance.ClockInTime.Value);
                 else return 0;
    
                 DateTime end;
                 if (_attendance.CheckOutTime.HasValue) end = _attendance.CheckOutTime.Value;
                 else end = DateTime.Now; 
    
                 if (start >= end) return 0;
    
                 double overtimeHours = 0;
                 string branch = _attendance.Branch ?? _employee.Branch ?? "Johannesburg";
                 
                 var current = start;
                 var interval = TimeSpan.FromMinutes(15);
                 
                 while (current < end)
                 {
                     var next = current.Add(interval);
                     if (next > end) next = end;
                     
                     var durationHours = (next - current).TotalHours;
                     var multiplier = GetMultiplier(current, branch);
                     
                     if (multiplier > 1.0)
                     {
                        overtimeHours += durationHours;
                     }
                     
                     current = next;
                 }
                 return overtimeHours;
            }
        }
        
        public string OvertimeHoursDisplay => OvertimeHours > 0 ? $"{OvertimeHours:F2}" : "-";

        private double GetMultiplier(DateTime time, string branch)
        {
            // 0. Public Holidays = 2.0x (Highest Priority)
            if (_isPublicHoliday) return 2.0;

            // 1. Sundays = 2.0x
            if (time.DayOfWeek == DayOfWeek.Sunday) return 2.0;

            // 2. Saturdays = 1.5x
            if (time.DayOfWeek == DayOfWeek.Saturday) return 1.5;

            // 3. Weekday Overtime (After 16:00 JHB / 17:00 CPT) = 1.5x
            int endHour = branch.Contains("Cape", StringComparison.OrdinalIgnoreCase) ? 17 : 16;
            
            // If before 07:00 start? Usually early starts are also OT, but focused on late for now.
            // Requirement was: "Weekdays after normal hours". 
            // Normal hours usually end at 16:00/17:00.
            if (time.Hour >= endHour) return 1.5;
            
            // Early morning? (Before 7/8). Assuming standard day matches Overtime logic.
            // For now, Standard Rate.
            return 1.0;
        }
        
        public string WageDisplay => $"{Wage:C}";

        public void Refresh()
        {
            OnPropertyChanged(nameof(HoursWorked));
            OnPropertyChanged(nameof(HoursWorkedDisplay));
            OnPropertyChanged(nameof(Wage));
            OnPropertyChanged(nameof(WageDisplay));
        }

        // Expose underlying data for Export
        public Employee Employee => _employee;
        public AttendanceRecord Attendance => _attendance;
    }
}
