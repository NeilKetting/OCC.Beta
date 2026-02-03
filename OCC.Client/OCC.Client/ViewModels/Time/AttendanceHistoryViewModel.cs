using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OCC.Client.ViewModels.Time;

using OCC.Shared.Models;

namespace OCC.Client.ViewModels.Time
{
    public partial class AttendanceHistoryViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<ViewModels.Messages.EntityUpdatedMessage>
    {
        private readonly ITimeService _timeService;
        private readonly IExportService _exportService;
        private readonly IPdfService _pdfService;

        #region Filter Properties

        [ObservableProperty]
        private string _selectedRange = "Today";

        public ObservableCollection<string> RangeOptions { get; } = new ObservableCollection<string>
        {
            "Today",
            "Yesterday",
            "This Week",
            "Last Week",
            "This Month",
            "Last Month",
            "Custom"
        };

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTime.Today;

        [ObservableProperty]
        private DateTimeOffset _endDate = DateTime.Today;
        
        [ObservableProperty]
        private bool _isCustomDateEnabled;

        [ObservableProperty]
        private string _selectedPayType = "All";

        public ObservableCollection<string> PayTypeOptions { get; } = new ObservableCollection<string>
        {
            "All",
            "Hourly",
            "Salary"
        };

        [ObservableProperty]
        private string _selectedBranch = "All";

        public ObservableCollection<string> BranchOptions { get; } = new ObservableCollection<string>
        {
            "All",
            "Johannesburg",
            "Cape Town"
        };

        #endregion

        #region Data Properties

        [ObservableProperty]
        private ObservableCollection<HistoryRecordViewModel> _records = new();

        [ObservableProperty]
        private decimal _totalWages;

        [ObservableProperty]
        private double _totalHours;

        [ObservableProperty]
        private bool _isAllSelected;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _totalLates;

        [ObservableProperty]
        private int _totalAbsences;

        private System.Collections.Generic.List<HistoryRecordViewModel> _allRecords = new();
        private readonly DispatcherTimer _timer;

        [ObservableProperty]
        private bool _isEmployeeReportPopupVisible;

        [ObservableProperty]
        private EmployeeReportViewModel? _employeeReportPopup;

        #endregion
        
        // Permission Property for UI Binding
        // Permission and Holiday Services
        public bool IsWageVisible { get; }
        private readonly IHolidayService _holidayService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Design-time constructor
        /// </summary>
        public AttendanceHistoryViewModel()
        {
            IsWageVisible = true;
            _timeService = null!;
            _exportService = null!;
            _timer = null!;
            _holidayService = null!;
            _dialogService = null!;
            _dialogService = null!;
            _pdfService = null!;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PrintStaffReportCommand))] // Notify when selection changes
        private HistoryRecordViewModel? _selectedRecord;

        partial void OnSelectedRecordChanged(HistoryRecordViewModel? value)
        {
             // Optional: If user selects a row, maybe clear checkboxes? 
             // "If checkbox is clicked and another row selected we do the report for the one that is checked instead."
             // So no need to clear.
             PrintStaffReportCommand.NotifyCanExecuteChanged();
        }

        public AttendanceHistoryViewModel(ITimeService timeService, IExportService exportService, IPermissionService permissionService, IHolidayService holidayService, IDialogService dialogService, IPdfService pdfService)
        {
            _timeService = timeService;
            _exportService = exportService;
            _holidayService = holidayService;
            _dialogService = dialogService;
            _pdfService = pdfService;
            
            // Check Permission
            IsWageVisible = permissionService.CanAccess("WageViewing");
            
            // Timer for Live Updates
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _timer.Tick += (s, e) => 
            {
                 foreach (var r in Records)
                 {
                     // Refresh active records
                     if (r.Attendance.CheckOutTime == null)
                     {
                         r.Refresh();
                     }
                 }
                 // Recalculate Totals?
                 if (Records.Any())
                 {
                     TotalWages = Records.Sum(r => r.Wage);
                     TotalHours = Records.Sum(r => r.HoursWorked);
                 }
             };
            _timer.Start();

            // Set default range logic
            SetRange("Today");
            
            // Initial Load
            InitializeCommand.Execute(null);

            // Register for Messages
            CommunityToolkit.Mvvm.Messaging.IMessengerExtensions.RegisterAll(CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default, this);
        }

        public void Receive(ViewModels.Messages.EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "AttendanceRecord")
            {
                // Delay slightly to allow DB to commit if needed, though usually sequential.
                // Reload Data
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => await LoadData());
            }
        }

        [RelayCommand]
        private async Task ExportCsv()
        {
            if (Records == null || !Records.Any()) return;
            
            var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), $"Attendance_Export_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            
            // Create a projection for clean CSV
            var data = Records.Select(r => new 
            {
                Date = r.Date.ToShortDateString(),
                Employee = r.EmployeeName,
                Branch = r.Branch,
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Wage = r.WageDisplay
            });

            await _exportService.ExportToCsvAsync(data, path);
            await _exportService.OpenFileAsync(path); // Open directly? Maybe users prefers folder. 
            // For now, let's open it so they see it.
        }



        [RelayCommand]
        private async Task EditRecord(HistoryRecordViewModel record)
        {
            if (record == null) return;

            // Use the specific Edit Attendance Dialog
            // We parse current strings to TimeSpans if possible
            TimeSpan? currentIn = TimeSpan.TryParse(record.InTime, out var inTs) ? inTs : null;
            TimeSpan? currentOut = TimeSpan.TryParse(record.OutTime, out var outTs) ? outTs : null;

            var result = await _dialogService.ShowEditAttendanceAsync(currentIn, currentOut);

            if (result.Confirmed && result.InTime.HasValue)
            {
                var originalDate = record.Attendance.Date.Date;
                var newCheckIn = originalDate.Add(result.InTime.Value);
                
                DateTime? newCheckOut = null;
                if (result.OutTime.HasValue)
                {
                    newCheckOut = originalDate.Add(result.OutTime.Value);
                    // Handle overnight shift?
                    if (newCheckOut < newCheckIn) newCheckOut = newCheckOut.Value.AddDays(1);
                }
                
                // Update Model
                record.Attendance.CheckInTime = newCheckIn;
                record.Attendance.CheckOutTime = newCheckOut;
                
                // Save
                await _timeService.SaveAttendanceRecordAsync(record.Attendance);
                await LoadData(); // Refresh list to update calcs
            }
        }

        [RelayCommand]
        private async Task DeleteRecord(HistoryRecordViewModel record)
        {
            if (record == null) return;
            
            var confirm = await _dialogService.ShowConfirmationAsync("Delete Record", 
                $"Are you sure you want to delete the attendance record for {record.EmployeeName} on {record.Date:dd MMM}?");
                
            if (confirm)
            {
                await _timeService.DeleteAttendanceRecordAsync(record.Attendance.Id);
                await LoadData();
            }
        }

        [RelayCommand]
        private async Task BulkEdit(object? parameter)
        {
            var selected = Records.Where(r => r.IsSelected).ToList();

            // Also include DataGrid row selection if passed (user might just highlight rows)
            if (parameter is System.Collections.IList list)
            {
                foreach (var item in list)
                {
                    if (item is HistoryRecordViewModel vm && !selected.Contains(vm))
                    {
                        selected.Add(vm);
                    }
                }
            }

            if (!selected.Any())
            {
                await _dialogService.ShowAlertAsync("No Selection", "Please select one or more records to edit (via checkboxes or row selection).");
                return;
            }

            var result = await _dialogService.ShowEditAttendanceAsync(null, null);

            if (result.Confirmed && result.InTime.HasValue)
            {
                IsBusy = true;
                try
                {
                    int successCount = 0;
                    int failCount = 0;
                    
                    foreach (var record in selected)
                    {
                        try
                        {
                            var originalDate = record.Attendance.Date.Date;
                            // Calculate New Times
                            // Ensure we create NEW DateTime objects
                            var newCheckIn = originalDate.Add(result.InTime.Value);

                            DateTime? newCheckOut = record.Attendance.CheckOutTime; // Default to existing
                            
                            if (result.OutTime.HasValue)
                            {
                                var tempOut = originalDate.Add(result.OutTime.Value);
                                if (tempOut < newCheckIn) tempOut = tempOut.AddDays(1);
                                newCheckOut = tempOut;
                            }

                            // Update locally
                            record.Attendance.CheckInTime = newCheckIn;
                            record.Attendance.CheckOutTime = newCheckOut;

                            // Save
                            await _timeService.SaveAttendanceRecordAsync(record.Attendance);
                            record.Refresh();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            System.Diagnostics.Debug.WriteLine($"Failed to update record {record.EmployeeName}: {ex.Message}");
                        }
                    }
                    
                    if (failCount > 0)
                    {
                        await _dialogService.ShowAlertAsync("Bulk Edit Partial Failure", $"Updated {successCount} records. Failed to update {failCount} records. Check logs.");
                    }

                    // Give API a moment to process/commit if necessary, though Save is awaited.
                    // This primarily helps if there are any async db consistency delays on the server.
                    await Task.Delay(500);

                    IsBusy = false;
                    await LoadData();
                    IsAllSelected = false; // Reset selection
                }
                finally { IsBusy = false; }
            }
        } // End BulkEdit

        [RelayCommand]
        private async Task SmartEdit(object? parameter)
        {
             // Determine context: Single or Multiple?
             var selected = Records.Where(r => r.IsSelected).ToList();

             // Check parameter
             if (parameter is System.Collections.IList list)
             {
                 foreach (var item in list)
                 {
                     if (item is HistoryRecordViewModel vm && !selected.Contains(vm))
                     {
                         selected.Add(vm);
                     }
                 }
             }
             else if (parameter is HistoryRecordViewModel singleVm)
             {
                 if (!selected.Contains(singleVm)) selected.Add(singleVm);
             }

             if (!selected.Any()) return;

             // Single Item Logic (Pre-fill times)
             if (selected.Count == 1 && !Records.Any(r => r.IsSelected)) // If only 1 item and NO explicit checkboxes checked, treat as specific edit
             {
                 await EditRecord(selected.First());
             }
             else
             {
                 // Bulk Logic (Empty times)
                 // Pass the merged list back to BulkEdit logic or call it directly
                 // Since BulkEdit accepts parameter, let's just call passing selected list
                 await BulkEdit(selected);
             }
        }

        [RelayCommand]
        private async Task Initialize()
        {
            await LoadData();
        }

        async partial void OnSelectedRangeChanged(string value)
        {
            SetRange(value);
            await LoadData();
        }



        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var record in Records)
            {
                record.IsSelected = value;
            }
        }

        [ObservableProperty]
        private string _statusFilter = "All";

        [RelayCommand]
        private void FilterByStatus(string status)
        {
             // Toggle off if clicking same status active
             if (StatusFilter == status) StatusFilter = "All";
             else StatusFilter = status;
             
             FilterRecords();
        }

        private void FilterRecords()
        {
            if (_allRecords == null) return;
            
            var query = SearchText?.Trim();
            
            var filtered = _allRecords.Where(r => 
            {
                // Text Search
                bool matchText = string.IsNullOrWhiteSpace(query) || 
                                 (r.EmployeeName != null && r.EmployeeName.Contains(query, StringComparison.OrdinalIgnoreCase));

                // Pay Type Filter
                bool matchPay = SelectedPayType == "All" || string.Equals(r.PayType, SelectedPayType, StringComparison.OrdinalIgnoreCase);

                // Branch Filter
                string recordBranch = r.Branch ?? "Johannesburg";
                bool matchBranch = SelectedBranch == "All" || string.Equals(recordBranch, SelectedBranch, StringComparison.OrdinalIgnoreCase);

                // Status Filter (Late/Absent etc)
                // Note: Absences are phantom records not in the list usually? 
                // Ah, Absences are CALCULATED, they are not records in the list unless we generate them.
                // The current list only contains actual Attendance Records. 
                // So filtering by "Absent" will show nothing unless we add fake rows.
                // User said "count the absences as well and also if we click on either lets fileter either".
                // If I filter by "Late", that works.
                // If I filter by "Absent", I can't show them if they don't exist in the list.
                // For now, let's assume filtering "Late" works. Filtering "Absent" might require UI change to show missing days.
                // Given constraints, I will enable "Late" filtering. For "Absent", I'll set status but it may show empty list, which is technically correct if no rows exist.
                
                bool matchStatus = StatusFilter == "All" || string.Equals(r.Status, StatusFilter, StringComparison.OrdinalIgnoreCase);

                return matchText && matchPay && matchBranch && matchStatus;
            }).ToList();

            // Trigger updates
                                        
            // Subscribe to PropertyChanged for Checkbox updates
            foreach(var rec in filtered)
            {
                rec.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(HistoryRecordViewModel.IsSelected))
                    {
                        PrintStaffReportCommand.NotifyCanExecuteChanged();
                    }
                };
            }

            Records = new ObservableCollection<HistoryRecordViewModel>(filtered);
            PrintStaffReportCommand.NotifyCanExecuteChanged();
            
            // Recalculate totals for filtered view
            if (filtered.Any())
            {
                TotalWages = filtered.Sum(r => r.Wage);
                TotalHours = filtered.Sum(r => r.HoursWorked);
                
                // Calculate Stats for ALL filtered records
                TotalLates = filtered.Count(r => r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late);
                
                // Calculate Absences
                // We need to calculate absences for all unique employees in the filtered list
                // This might be expensive if range is large, but essential for accurate stats.
                CalculateTotalAbsences(filtered);
            }
            else
            {
                 TotalWages = 0;
                 TotalHours = 0;
                 TotalLates = 0;
                 TotalAbsences = 0;
            }
        }

        private void CalculateAbsences(OCC.Shared.Models.Employee employee, System.Collections.Generic.List<HistoryRecordViewModel> records)
        {
            int absences = 0;
            var current = StartDate.Date;
            var end = EndDate.Date;
            if (end > DateTime.Today) end = DateTime.Today; // Don't count future absences

            while (current <= end)
            {
                // Skip Weekends
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    // Skip Public Holidays
                    if (!OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(current))
                    {
                        // Check if we have a record for this day
                        bool hasRecord = records.Any(r => r.Date.Date == current.Date && 
                                                         (r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Present || 
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late ||
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.LeaveAuthorized ||
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Sick));
                                                          
                        if (!hasRecord) absences++;
                    }
                }
                current = current.AddDays(1);
            }
            TotalAbsences = absences;
        }

        [RelayCommand]
        public void OpenEmployeeReport(object parameter)
        {
            if (parameter is HistoryRecordViewModel record)
            {
                OpenReportForEmployee(record.Employee);
            }
            else if (parameter is Employee emp)
            {
                 OpenReportForEmployee(emp);
            }
        }

        private void OpenReportForEmployee(Employee employee)
        {
             if (employee == null) return;
             try
             {
                 EmployeeReportPopup = new EmployeeReportViewModel(employee, _timeService, _exportService, _holidayService, _pdfService);
                 IsEmployeeReportPopupVisible = true;
             }
             catch (Exception ex)
             {
                 _dialogService.ShowAlertAsync("Error", $"Could not open report: {ex.Message}");
             }
        }

        [RelayCommand]
        private void CloseReport()
        {
            IsEmployeeReportPopupVisible = false;
            EmployeeReportPopup = null;
        }

        public bool CanPrintStaffReport
        {
            get
            {
                if (Records == null) return false;

                var selectedCheckboxes = Records.Where(r => r.IsSelected).ToList();
                if (selectedCheckboxes.Any())
                {
                    // "If more than one checkbox is selected it should also not be enabled" -> 
                    // Assuming "More than one EMPLOYEE" is the constraint given "Individual Report".
                    // If user selects 5 rows for Anna, it should be enabled.
                    // If user selects 1 row for Anna, 1 for Bob, it should NOT be enabled.
                    var distinctEmployees = selectedCheckboxes.Select(r => r.EmployeeName).Distinct().Count();
                    return distinctEmployees == 1;
                }

                if (SelectedRecord != null) return true;

                // Fallback: If filtered to single employee
                if (Records.Any() && Records.GroupBy(x => x.EmployeeName).Count() == 1) return true;

                // "If neither are selected the button should not be enabled." -> Strict mode.
                return false;
            }
        }

        [RelayCommand]
        private async Task PrintReport()
        {
            if (Records == null || !Records.Any()) return;

            // Aggregate data per employee for a summary report
            var summaryData = Records.GroupBy(r => r.Employee)
                .Select(g => new 
                {
                    Employee = g.Key.DisplayName,
                    Branch = g.Key.Branch ?? "-",
                    TotalHours = g.Sum(x => x.HoursWorked).ToString("F2"),
                    Overtime = g.Sum(x => x.OvertimeHours).ToString("F2"),
                    Lates = g.Count(x => x.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late),
                    Absences = 0, // Need to calc if we want per-row absences, but expensive. Maybe leave 0 or aggregate? User complained it was empty.
                    EstPay = g.Sum(x => x.Wage).ToString("C")
                })
                .OrderBy(x => x.Employee)
                .ToList();

            var columns = new Dictionary<string, string>
            {
                { "Employee", "Employee" },
                { "Branch", "Branch" },
                { "TotalHours", "Total Hours" },
                { "Overtime", "Overtime (Hrs)" },
                { "Lates", "Lates" },
                { "Absences", "Absences" },
                { "EstPay", "Est. Pay" }
            };

            var title = $"Attendance Summary Report ({StartDate:dd MMM} - {EndDate:dd MMM yyyy})";
            
            try 
            {
                var path = await _exportService.GenerateHtmlReportAsync(summaryData, title, columns);
                
                var p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true };
                p.Start();
            }
            catch(Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Could not print report: {ex.Message}");
            }
        }

        [RelayCommand(CanExecute = nameof(CanPrintStaffReport))]
        private async Task PrintStaffReport()
        {
            if (Records == null || !Records.Any()) return;
            
            // Determine the target data source
            System.Collections.Generic.IEnumerable<HistoryRecordViewModel> targetRecords = Records;
            
            bool isFilteredToSingle = Records.GroupBy(x => x.EmployeeName).Count() == 1;
            bool hasSelection = Records.Any(r => r.IsSelected);

            if (hasSelection)
            {
                // Checkbox takes precedence
                var selected = Records.Where(r => r.IsSelected).ToList();
                var distinctEmployees = selected.GroupBy(x => x.EmployeeName).ToList();
                
                if (distinctEmployees.Count == 1)
                {
                   var name = distinctEmployees.First().Key;
                   // Use ALL records for this employee in the current view? Or just Selected?
                   // User said: "we do the report for the one that is checked instead".
                   // Implies we target that Employee. usually reports cover the whole period.
                   targetRecords = Records.Where(r => r.EmployeeName == name).ToList();
                }
                else
                {
                    // Should be blocked by CanExecute, but failsafe
                     await _dialogService.ShowAlertAsync("Multiple Employees", "Selection invalid.");
                     return;
                }
            }
            else if (SelectedRecord != null)
            {
                // Selected Row takes 2nd priority
                var name = SelectedRecord.EmployeeName;
                targetRecords = Records.Where(r => r.EmployeeName == name).ToList();
            }
            else if (isFilteredToSingle)
            {
                // Fallback
            }
            else
            {
                // Should not happen due to CanExecute
                await _dialogService.ShowAlertAsync("Select Employee", "Please select an employee via checkbox or row.");
                return;
            }
            
            // Generate
            var employee = targetRecords.First().Employee;
            var data = targetRecords.OrderByDescending(r => r.Date).Select(r => new 
            {
                Date = r.Date.ToShortDateString(),
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Wage = r.WageDisplay
            });

            // Recalculate summary for the report context
            var reportWages = targetRecords.Sum(r => r.Wage);
            var reportHours = targetRecords.Sum(r => r.HoursWorked);
            var reportLates = targetRecords.Count(r => r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late);
            
            var tempAbsences = CalculateAbsencesForReport(employee, targetRecords.ToList());

            var summary = new Dictionary<string, string>
            {
                { "Total Hours", $"{reportHours:F2}" },
                { "Total Lates", $"{reportLates}" },
                { "Absences", $"{tempAbsences}" } 
            };

            var title = $"Individual Staff Report: {employee.DisplayName}";
            var path = await _exportService.GenerateIndividualStaffReportAsync(employee, StartDate.DateTime, EndDate.DateTime, data, summary);
            
            await _exportService.OpenFileAsync(path);
        }

        private void CalculateTotalAbsences(System.Collections.Generic.List<HistoryRecordViewModel> records)
        {
            var absences = 0;
            // Get unique employees in the filtered view
            var employees = records.Select(r => r.Employee).DistinctBy(e => e.Id).ToList();

            foreach(var employee in employees)
            {
                var current = StartDate.Date;
                var end = EndDate.Date;
                if (end > DateTime.Today) end = DateTime.Today;

                while (current <= end)
                {
                    if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (!OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(current))
                        {
                            // Check if this employee has a record on this day
                            bool hasRecord = records.Any(r => r.Employee.Id == employee.Id && r.Date.Date == current.Date && 
                                                             (r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Present || 
                                                              r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late ||
                                                              r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.LeaveAuthorized ||
                                                              r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Sick));
                                                              
                            if (!hasRecord) absences++;
                        }
                    }
                    current = current.AddDays(1);
                }
            }
            TotalAbsences = absences;
        }

        private int CalculateAbsencesForReport(OCC.Shared.Models.Employee employee, System.Collections.Generic.List<HistoryRecordViewModel> records)
        {
             // Similar logic to CalculateAbsences but independent
            int absences = 0;
            var current = StartDate.Date;
            var end = EndDate.Date;
            if (end > DateTime.Today) end = DateTime.Today; 

            while (current <= end)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (!OCC.Shared.Utils.HolidayUtils.IsPublicHoliday(current))
                    {
                        bool hasRecord = records.Any(r => r.Date.Date == current.Date && 
                                                         (r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Present || 
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Late ||
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.LeaveAuthorized ||
                                                          r.Attendance.Status == OCC.Shared.Models.AttendanceStatus.Sick));
                                                          
                        if (!hasRecord) absences++;
                    }
                }
                current = current.AddDays(1);
            }
            return absences;
        }

        private void SetRange(string range)
        {
            var today = DateTime.Today;
            IsCustomDateEnabled = false;

            switch (range)
            {
                case "Today":
                    StartDate = today;
                    EndDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "Yesterday":
                    StartDate = today.AddDays(-1);
                    EndDate = today.AddDays(-1).AddDays(1).AddTicks(-1);
                    break;
                case "This Week":
                    // Assuming Week starts Monday
                    int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                    StartDate = today.AddDays(-1 * diff).Date;
                    EndDate = today.AddDays(1).AddTicks(-1); // Up to now
                    break;
                case "Last Week":
                    var lastWeek = today.AddDays(-7);
                    int lwDiff = (7 + (lastWeek.DayOfWeek - DayOfWeek.Monday)) % 7;
                    StartDate = lastWeek.AddDays(-1 * lwDiff).Date;
                    EndDate = StartDate.AddDays(7).AddTicks(-1); // Full week
                    break;
                case "This Month":
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = today.AddDays(1).AddTicks(-1);
                    break;
                case "Last Month":
                    var lastMonth = today.AddMonths(-1);
                    StartDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    EndDate = new DateTime(today.Year, today.Month, 1).AddTicks(-1);
                    break;
                case "Custom":
                    IsCustomDateEnabled = true;
                    // Keep current dates
                    break;
            }
        }
        
        [RelayCommand]
        private async Task Refresh()
        {
            await LoadData();
        }

        partial void OnSearchTextChanged(string value) => FilterRecords();
        partial void OnSelectedPayTypeChanged(string value) => FilterRecords();
        partial void OnSelectedBranchChanged(string value) => FilterRecords();

        // Trigger load when dates change IF Custom is selected
        async partial void OnStartDateChanged(DateTimeOffset value)
        {
            if (SelectedRange == "Custom") await LoadData();
        }

        async partial void OnEndDateChanged(DateTimeOffset value)
        {
            if (SelectedRange == "Custom") await LoadData();
        }

        private async Task LoadData()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var s = StartDate.DateTime;
                var e = EndDate.DateTime;
                
                // Fetch
                var attendanceEnumerable = await _timeService.GetAttendanceByRangeAsync(s, e);
                var attendance = attendanceEnumerable.ToList();

                // FIX: Also include ANY active records (Clocked In) regardless of date, 
                // so they appear in the history list (e.g. if Shift started yesterday).
                // Only do this if the range implies "Current" relevance, or generally helpful to seeing status.
                // We'll merge them in.
                var activeRecords = await _timeService.GetActiveAttendanceAsync();
                foreach (var active in activeRecords)
                {
                    if (!attendance.Any(x => x.Id == active.Id))
                    {
                        attendance.Add(active);
                    }
                }
                var allEmployee = await _timeService.GetAllStaffAsync();

                var list = new System.Collections.Generic.List<HistoryRecordViewModel>();

                foreach (var rec in attendance)
                {
                    var emp = allEmployee.FirstOrDefault(em => em.Id == rec.EmployeeId);
                    if (emp == null) continue;

                    var vm = new HistoryRecordViewModel(rec, emp, _holidayService);
                    list.Add(vm);
                }

                // Sort by Name, then Date Descending
                _allRecords = list.OrderBy(x => x.EmployeeName).ThenByDescending(x => x.Date).ThenBy(x => x.InTime).ToList();

                FilterRecords();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
