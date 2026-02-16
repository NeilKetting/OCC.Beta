using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class HistoryRecordViewModel : ObservableObject
    {
        private readonly IHolidayService _holidayService;
        private readonly AttendanceRecord _attendance;
        private readonly Employee _employee;
        private bool _isPublicHoliday;
        private readonly BranchDetails? _branchDetails; // Nullable if default needed
        
        [ObservableProperty]
        private bool _isSelected;
        
        // Expanded Overtime Properties
        [ObservableProperty]
        private double _overtimeHours15;

        [ObservableProperty]
        private double _overtimeHours20;

        [ObservableProperty]
        private decimal _overtimePay15;

        [ObservableProperty]
        private decimal _overtimePay20;

        public HistoryRecordViewModel(AttendanceRecord attendance, Employee employee, IHolidayService holidayService, BranchDetails? branchDetails = null)
        {
            _attendance = attendance;
            _employee = employee;
            _holidayService = holidayService;
            _branchDetails = branchDetails;
            
            // Async check for holiday status
            CheckHolidayStatus();
            
            // Initial Calc
            CalculateEverything();
        }

        private async void CheckHolidayStatus()
        {
            if (_attendance.CheckInTime.HasValue || _attendance.ClockInTime.HasValue)
            {
                var d = _attendance.CheckInTime.HasValue ? _attendance.CheckInTime.Value.Date : _attendance.Date;
                _isPublicHoliday = await _holidayService.IsHolidayAsync(d);
                if (_isPublicHoliday)
                {
                    CalculateEverything(); // Recalculate
                }
            }
        }
        
        // Triggers calculation of all derived values
        private void CalculateEverything()
        {
             CalculateAccurateWage(); // This will populate prop fields
        }

        public DateTime Date => _attendance.Date;
        public string EmployeeName => _employee.DisplayName;
        public string Branch => _attendance.Branch; 
        public string PayType => _employee.RateType == RateType.Hourly ? "Hourly" : "Salary";
        
        public string InTime => _attendance.CheckInTime?.ToString("HH:mm") ?? _attendance.ClockInTime?.ToString(@"hh\:mm") ?? "--:--";
        
        public string OutTime 
        {
            get
            {
                if (_attendance.CheckOutTime == null || _attendance.CheckOutTime == DateTime.MinValue) return "--:--";
                // Sentinel Check: If midnight on same day or day after InTime, it's active
                if (_attendance.CheckOutTime.Value.TimeOfDay.TotalSeconds < 1)
                {
                    var date = _attendance.Date.Date;
                    var checkoutDate = _attendance.CheckOutTime.Value.Date;
                    if (checkoutDate == date || checkoutDate == date.AddDays(1)) return "--:--";
                }
                return _attendance.CheckOutTime.Value.ToString("HH:mm");
            }
        }

        public string Status => _attendance.Status.ToString();

        // Calculations
        [ObservableProperty]
        private double _hoursWorked;

        [ObservableProperty]
        private decimal _wage;
        
        public string HoursWorkedDisplay => HoursWorked > 0 ? $"{HoursWorked:F2}" : "-";
        public string WageDisplay => $"{Wage:C}";
        
        public double LunchDeduction
        {
            get
            {
                 // We calculate effective lunch in the main loop, but if we need a display property:
                 // It's just the overlap of Effective Time with 12:00-13:00.
                 var (start, end) = GetEffectiveTimes();
                 if (start >= end) return 0;
                 
                 DateTime lunchStart = start.Date.AddHours(12);
                 DateTime lunchEnd = start.Date.AddHours(13);
                 
                 var intersectStart = start > lunchStart ? start : lunchStart;
                 var intersectEnd = end < lunchEnd ? end : lunchEnd;
                 
                 if (intersectStart < intersectEnd)
                     return (intersectEnd - intersectStart).TotalHours;
                     
                 return 0;
            }
        }
        
        public string LunchDeductionDisplay => LunchDeduction > 0 ? $"{LunchDeduction:F2}h" : "-";

        [ObservableProperty]
        private double _overtimeHours;
        
        public string OvertimeHoursDisplay => OvertimeHours > 0 ? $"{OvertimeHours:F2}" : "-";

        private (DateTime Start, DateTime End) GetEffectiveTimes()
        {
             DateTime rawStart;
             if (_attendance.CheckInTime.HasValue) rawStart = _attendance.CheckInTime.Value;
             else if (_attendance.ClockInTime.HasValue) rawStart = _attendance.Date.Add(_attendance.ClockInTime.Value);
             else return (DateTime.MinValue, DateTime.MinValue);
 
             DateTime rawEnd;
             // If CheckOutTime is null, MinValue, or exactly 00:00 on the same day OR next day, it's NOT a real checkout (it's active)
             bool isActive = !_attendance.CheckOutTime.HasValue || 
                             _attendance.CheckOutTime.Value == DateTime.MinValue || 
                             (_attendance.CheckOutTime.Value.TimeOfDay.TotalSeconds < 1 && 
                              (_attendance.CheckOutTime.Value.Date == _attendance.Date.Date || _attendance.CheckOutTime.Value.Date == _attendance.Date.AddDays(1)));

             if (!isActive) rawEnd = _attendance.CheckOutTime!.Value;
             else rawEnd = DateTime.Now; 

             if (rawStart >= rawEnd) return (DateTime.MinValue, DateTime.MinValue);
             
             // Priority: Employee -> Branch -> Default
             TimeSpan shiftStart = _employee.ShiftStartTime ?? _branchDetails?.ShiftStartTime ?? new TimeSpan(7,0,0);
             TimeSpan shiftEnd = _employee.ShiftEndTime ?? _branchDetails?.ShiftEndTime ?? new TimeSpan(16,45,0);

             // Construct Shift DateTimes
             DateTime shiftStartDt = rawStart.Date.Add(shiftStart);
             DateTime shiftEndDt = rawStart.Date.Add(shiftEnd);

             // Snap Logic
             // In: If within 15m BEFORE ShiftStart, Snap to ShiftStart
             DateTime effectiveIn = rawStart;
             if (rawStart < shiftStartDt && (shiftStartDt - rawStart).TotalMinutes <= 15)
             {
                 effectiveIn = shiftStartDt;
             }
             
             // Out: If within 15m BEFORE or AFTER ShiftEnd, Snap to ShiftEnd
             DateTime effectiveOut = rawEnd;
             if (Math.Abs((rawEnd - shiftEndDt).TotalMinutes) <= 15)
             {
                 effectiveOut = shiftEndDt;
             }
             
             return (effectiveIn, effectiveOut);
        }


        private void CalculateAccurateWage()
        {
             var (start, end) = GetEffectiveTimes();
             if (start == DateTime.MinValue) 
             {
                 HoursWorked = 0;
                 Wage = 0;
                 OvertimeHours = 0;
                 OvertimeHours15 = 0;
                 OvertimeHours20 = 0;
                 return;
             }

             if (_employee.RateType == RateType.Hourly)
             {
                  CalculateHourlyWage(start, end);
             }
             else
             {
                  // Salary Logic (Simplified for now, borrowing structure)
                  // Salary usually just pays fixed, but if tracking OT:
                  CalculateHourlyWage(start, end); // Reuse for tracking, but maybe override Wage?
                  // Re-apply Salary Wage Override if needed (not requested to change, but OT needs tracking)
                  // For now, let's assume we calculate "Value" same way.
             }
        }
        
        private void CalculateHourlyWage(DateTime start, DateTime end)
        {
             decimal totalWage = 0;
             double totalHours = 0;
             double ot15 = 0;
             double ot20 = 0;
             decimal otPay15 = 0;
             decimal otPay20 = 0;

             decimal rateToUse = _attendance.CachedHourlyRate ?? (decimal)_employee.HourlyRate;
             if (rateToUse == 0)
             {
                 Serilog.Log.Warning("[HistoryVM] Wage is zero for {Name}. Employee Rate: {EmpRate}, Cached Rate: {CachedRate}", 
                     _employee.DisplayName, _employee.HourlyRate, _attendance.CachedHourlyRate);
             }
             double hourlyRate = (double)rateToUse;
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
                    totalHours += durationHours;
                    var segmentPay = (decimal)(durationHours * hourlyRate * multiplier);
                    totalWage += segmentPay;
                    
                    if (multiplier == 1.5) 
                    {
                        ot15 += durationHours;
                        otPay15 += segmentPay;
                    }
                    else if (multiplier == 2.0) 
                    {
                        ot20 += durationHours;
                        otPay20 += segmentPay;
                    }
                 }
                 
                 current = next;
             }
             
             HoursWorked = totalHours;
             Wage = totalWage;
             OvertimeHours15 = ot15;
             OvertimeHours20 = ot20;
             OvertimePay15 = otPay15;
             OvertimePay20 = otPay20;
             OvertimeHours = ot15 + ot20;
        }

        // ... Multiplier Logic same as before ...
        private double GetMultiplier(DateTime time, string branch)
        {
            // 0. Public Holidays = 2.0x (Highest Priority)
            if (_isPublicHoliday) return 2.0;

            // 1. Sundays = 2.0x
            if (time.DayOfWeek == DayOfWeek.Sunday || OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(time)) return 2.0;

            // 2. Saturdays = 1.5x
            if (time.DayOfWeek == DayOfWeek.Saturday) return 1.5;

            // 3. Weekday Overtime (After Shift End)
            // Use Branch Details if available, else fallback
            // 3. Weekday Overtime (After Shift End)
            // Priority: Employee Specific -> Branch -> Default
            TimeSpan shiftEnd = _employee.ShiftEndTime ?? _branchDetails?.ShiftEndTime ?? new TimeSpan(16, 45, 0);
            TimeSpan shiftStart = _employee.ShiftStartTime ?? _branchDetails?.ShiftStartTime ?? new TimeSpan(7, 0, 0);
            
            // Standard Overtime: Weekdays after shift
            if (time.TimeOfDay >= shiftEnd) return 1.5;
            
            // Early Work?
            if (time.TimeOfDay < shiftStart) return 1.5;

            return 1.0;
        }
        
        // Remove old properties that were computed properties, as we now set them
        // (Calculations region replaced above)
        
        public void Refresh()
        {
            CheckHolidayStatus(); 
            // Calculated properties notify themselves when set
            OnPropertyChanged(nameof(HoursWorked));
            OnPropertyChanged(nameof(HoursWorkedDisplay));
            OnPropertyChanged(nameof(Wage));
            OnPropertyChanged(nameof(WageDisplay));
            OnPropertyChanged(nameof(OvertimeHours));
            OnPropertyChanged(nameof(OvertimeHoursDisplay));
            OnPropertyChanged(nameof(OvertimeHours15));
            OnPropertyChanged(nameof(OvertimeHours20));

            // Force recalculate on refresh as well
            CalculateEverything();
        }

        // Expose underlying data for Export
        public Employee Employee => _employee;
        public AttendanceRecord Attendance => _attendance;
    }
}
