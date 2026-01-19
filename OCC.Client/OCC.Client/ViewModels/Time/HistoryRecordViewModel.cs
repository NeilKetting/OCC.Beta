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
        
        [ObservableProperty]
        private bool _isSelected;

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
                if (!checkOut.HasValue) checkOut = DateTime.Now;

                double rawHours = 0;
                if (_attendance.CheckInTime.HasValue)
                {
                    if (checkOut.HasValue)
                        rawHours = (checkOut.Value - _attendance.CheckInTime.Value).TotalHours;
                }
                else if (_attendance.ClockInTime.HasValue)
                {
                    var inDt = _attendance.Date.Add(_attendance.ClockInTime.Value);
                    if (checkOut.HasValue)
                        rawHours = (checkOut.Value - inDt).TotalHours;
                }

                if (rawHours <= 0) return 0;

                // Subtract mandatory lunch
                double netHours = rawHours - LunchDeduction;
                return netHours > 0 ? netHours : 0;
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
             else end = DateTime.Now; 
 
             if (start >= end || _attendance.Status == AttendanceStatus.Absent) return 0;
 
             decimal totalWage = 0;
             decimal rateToUse = _attendance.CachedHourlyRate ?? (decimal)_employee.HourlyRate;
             double hourlyRate = (double)rateToUse;
             string branch = _attendance.Branch ?? _employee.Branch ?? "Johannesburg";
             
             // UNPAID LUNCH WINDOW (12:00 - 13:00)
             DateTime lunchStart = start.Date.AddHours(12);
             DateTime lunchEnd = start.Date.AddHours(13);

             var current = start;
             var interval = TimeSpan.FromMinutes(15);
             
             while (current < end)
             {
                 var next = current.Add(interval);
                 if (next > end) next = end;
                 
                 var durationHours = (next - current).TotalHours;
                 
                 // Check if this interval overlaps lunch
                 var intersectStart = current > lunchStart ? current : lunchStart;
                 var intersectEnd = next < lunchEnd ? next : lunchEnd;
                 
                 if (intersectStart < intersectEnd)
                 {
                     // Skip this chunk of time for pay
                     // We could subtract just the intersection but chunking makes it simpler
                     durationHours -= (intersectEnd - intersectStart).TotalHours;
                 }

                 if (durationHours > 0)
                 {
                    var multiplier = GetMultiplier(current, branch);
                    totalWage += (decimal)(durationHours * hourlyRate * multiplier);
                 }
                 
                 current = next;
             }
             
             return totalWage;
        }

        public double LunchDeduction
        {
            get
            {
                DateTime start;
                if (_attendance.CheckInTime.HasValue) start = _attendance.CheckInTime.Value;
                else if (_attendance.ClockInTime.HasValue) start = _attendance.Date.Add(_attendance.ClockInTime.Value);
                else return 0;

                DateTime end;
                if (_attendance.CheckOutTime.HasValue) end = _attendance.CheckOutTime.Value;
                else end = DateTime.Now;

                DateTime lunchStart = start.Date.AddHours(12);
                DateTime lunchEnd = start.Date.AddHours(13);

                var intersectStart = start > lunchStart ? start : lunchStart;
                var intersectEnd = end < lunchEnd ? end : lunchEnd;

                if (intersectStart < intersectEnd)
                {
                    return (intersectEnd - intersectStart).TotalHours;
                }
                return 0;
            }
        }
        
        public string LunchDeductionDisplay => LunchDeduction > 0 ? $"{LunchDeduction:F2}h" : "-";

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
    
                  if (start >= end || _attendance.Status == AttendanceStatus.Absent) return 0;
     
                  double overtimeHours = 0;
                  string branch = _attendance.Branch ?? _employee.Branch ?? "Johannesburg";
                  
                  DateTime lunchStart = start.Date.AddHours(12);
                  DateTime lunchEnd = start.Date.AddHours(13);

                  var current = start;
                  var interval = TimeSpan.FromMinutes(15);
                  
                  while (current < end)
                  {
                      var next = current.Add(interval);
                      if (next > end) next = end;
                      
                      var durationHours = (next - current).TotalHours;
                      
                      // Check lunch
                      var intersectStart = current > lunchStart ? current : lunchStart;
                      var intersectEnd = next < lunchEnd ? next : lunchEnd;
                      if (intersectStart < intersectEnd)
                      {
                          durationHours -= (intersectEnd - intersectStart).TotalHours;
                      }

                      if (durationHours > 0)
                      {
                          var multiplier = GetMultiplier(current, branch);
                          if (multiplier > 1.0)
                          {
                             overtimeHours += durationHours;
                          }
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
            if (time.DayOfWeek == DayOfWeek.Sunday || OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(time)) return 2.0;

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
