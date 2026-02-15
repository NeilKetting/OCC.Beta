using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.Features.EmployeeHub.ViewModels;
using OCC.Client.Features.TimeAttendanceHub.ViewModels;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class EmployeeReportViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;
        private readonly IExportService _exportService;
        private readonly IHolidayService _holidayService;

        public Employee Employee { get; }

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<HistoryRecordViewModel> _records = new();

        [ObservableProperty]
        private double _totalHours;

        [ObservableProperty]
        private double _totalOvertime;

        [ObservableProperty]
        private double _totalOvertime15;

        [ObservableProperty]
        private double _totalOvertime20;

        [ObservableProperty]
        private int _totalLates;

        [ObservableProperty]
        private int _totalAbsences;

        [ObservableProperty]
        private decimal _totalPay;

        [ObservableProperty]
        private decimal _totalNormalPay;

        [ObservableProperty]
        private bool _isLoading;
        
        [ObservableProperty]
        private decimal _totalOvertimePay15;

        [ObservableProperty]
        private decimal _totalOvertimePay20;

        private readonly IPdfService _pdfService;

        public EmployeeReportViewModel(
            Employee employee,
            ITimeService timeService,
            IExportService exportService,
            IHolidayService holidayService,
            IPdfService pdfService)
        {
            Employee = employee;
            _timeService = timeService;
            _exportService = exportService;
            _holidayService = holidayService;
            _pdfService = pdfService;

            _ = LoadData();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadData();
        }

        partial void OnStartDateChanged(DateTime value) { _ = LoadData(); }
        partial void OnEndDateChanged(DateTime value) { _ = LoadData(); }

        public async Task LoadData()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var s = StartDate;
                var e = EndDate;

                // 1. Fetch Attendance for range
                var attendanceEnumerable = await _timeService.GetAttendanceByRangeAsync(s, e);
                
                // Filter for THIS employee
                var empAttendance = attendanceEnumerable
                                    .Where(a => a.EmployeeId == Employee.Id)
                                    .ToList();

                var list = new List<HistoryRecordViewModel>();
                
                // Process every day in the range to catch Absences
                var current = s.Date;
                var end = e.Date;
                if (end > DateTime.Today) end = DateTime.Today; // Don't project into future

                // We need to handle the case where "To" date is in the future, 
                // but we only show absences up to Today. 
                // However, if we have records in the future (unlikely but possible), show them?
                // Standard logic: Loop from Start to End.
                
                // Create lookup for performance
                var attendanceLookup = empAttendance.ToLookup(a => a.Date.Date);

                // Use the loop end as selected end date, but cap "Absence Creation" at Today
                for (var d = s.Date; d <= e.Date; d = d.AddDays(1))
                {
                    if (attendanceLookup.Contains(d))
                    {
                        // Add actual records
                        foreach(var rec in attendanceLookup[d])
                        {
                            list.Add(new HistoryRecordViewModel(rec, Employee, _holidayService));
                        }
                    }
                    else
                    {
                        // No record found. Check if we should insert an "Absent" record.
                        // Only insert if <= Today
                        if (d <= DateTime.Today)
                        {
                            // Check if it's a working day
                             bool isHoliday = OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(d);
                             bool isWeekend = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;
                             
                             if (!isWeekend && !isHoliday)
                             {
                                 // Create a synthetic Absent record for display
                                 var absentRecord = new AttendanceRecord
                                 {
                                     Id = Guid.NewGuid(), // Dummy ID
                                     EmployeeId = Employee.Id,
                                     Date = d,
                                     Status = AttendanceStatus.Absent,
                                     Branch = Employee.Branch
                                 };
                                 list.Add(new HistoryRecordViewModel(absentRecord, Employee, _holidayService));
                             }
                        }
                    }
                }

                // Add any out-of-range records (e.g. if logic above missed something active)
                // The loop should cover it.
                
                // Sort Descending
                Records = new ObservableCollection<HistoryRecordViewModel>(list.OrderByDescending(x => x.Date));

                // 2. Calculate Stats
                CalculateStats(Records.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading employee report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateStats(List<HistoryRecordViewModel> records)
        {
            TotalHours = records.Sum(r => r.HoursWorked);
            TotalOvertime = records.Sum(r => r.OvertimeHours);
            TotalOvertime15 = records.Sum(r => r.OvertimeHours15);
            TotalOvertime20 = records.Sum(r => r.OvertimeHours20);
            
            TotalOvertimePay15 = records.Sum(r => r.OvertimePay15);
            TotalOvertimePay20 = records.Sum(r => r.OvertimePay20);
            
            TotalLates = records.Count(r => r.Attendance.Status == AttendanceStatus.Late);
            TotalPay = records.Sum(r => r.Wage);
            
            TotalNormalPay = TotalPay - (TotalOvertimePay15 + TotalOvertimePay20);
            
            // Absences are now explicit records with Status == Absent
            TotalAbsences = records.Count(r => r.Attendance.Status == AttendanceStatus.Absent);
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            if (Records == null || !Records.Any()) return;

            var data = Records.OrderByDescending(r => r.Date).Select(r => new
            {
                Date = r.Date.ToShortDateString(),
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Wage = r.WageDisplay
            });

            // Calculate Normal Hours/Pay (Approximation for Display)
            // Total Hours - Total Overtime Hours
            var normalHours = TotalHours - TotalOvertime;
            // Total Pay - Total Overtime Pay
            var normalPay = TotalPay - (TotalOvertimePay15 + TotalOvertimePay20);

            var title = $"Individual Staff Report: {Employee.DisplayName}";
            
            try 
            {
                // Ensure we use the correct method signature from IExportService
                // Switch to PDF Service
                var summary = new Dictionary<string, string>
                {
                    { "Total Hours", $"{TotalHours:F2}" },
                    { "Normal Hours Pay", $"{normalPay:C}" }, // New Key for PDF Service
                    { "Total Overtime", $"{TotalOvertime:F2}" },
                     // Add breakdown to summary dictionary if PDF service expects it, 
                     // OR rely on PDF service to accept raw data.
                     // The Interface signature expected Dictionary.
                     // Let's add them to dictionary for now.
                    { "Overtime (1.5x)", $"{TotalOvertime15:F2}|{TotalOvertimePay15:C}" },
                    { "Overtime (2.0x)", $"{TotalOvertime20:F2}|{TotalOvertimePay20:C}" },
                    { "Total Lates", $"{TotalLates}" },
                    { "Absences", $"{TotalAbsences}" },
                    { "Gross Pay", $"{TotalPay:C}" }
                };

                var path = await _pdfService.GenerateEmployeeReportPdfAsync(Employee, StartDate, EndDate, data, summary);
                
                // Open PDF
                var p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
                p.Start();
            }
            catch(Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error generating report: {ex.Message}");
            }
        }
    }
}
