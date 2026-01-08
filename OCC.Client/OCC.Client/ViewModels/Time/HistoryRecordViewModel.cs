using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;

namespace OCC.Client.ViewModels.Time
{
    public partial class HistoryRecordViewModel : ObservableObject
    {
        private readonly AttendanceRecord _attendance;
        private readonly Employee _employee;

        public HistoryRecordViewModel(AttendanceRecord attendance, Employee employee)
        {
            _attendance = attendance;
            _employee = employee;
        }

        public DateTime Date => _attendance.Date;
        public string EmployeeName => _employee.DisplayName;
        public string Branch => _attendance.Branch; // Or employee branch? Attendance branch preserves history.
        
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
                    return (decimal)(HoursWorked * _employee.HourlyRate);
                }
                // RateType.MonthlySalary
                // Return 0 or specific logic? 
                // User requirement: "wages for the given period". 
                // For now, return 0 for Salary to avoid misleading "per hour" numbers.
                return 0;
            }
        }
        
        public string WageDisplay => _employee.RateType == RateType.Hourly ? $"{Wage:C}" : "Salary";

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
