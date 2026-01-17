using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class AttendanceHistoryViewModel : ViewModelBase, CommunityToolkit.Mvvm.Messaging.IRecipient<ViewModels.Messages.EntityUpdatedMessage>
    {
        private readonly ITimeService _timeService;
        private readonly IExportService _exportService;

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
        private string _searchText = string.Empty;

        private System.Collections.Generic.List<HistoryRecordViewModel> _allRecords = new();
        private readonly DispatcherTimer _timer;

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
        }

        public AttendanceHistoryViewModel(ITimeService timeService, IExportService exportService, IPermissionService permissionService, IHolidayService holidayService, IDialogService dialogService)
        {
            _timeService = timeService;
            _exportService = exportService;
            _holidayService = holidayService;
            _dialogService = dialogService;
            
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
        private async Task PrintReport()
        {
            if (Records == null || !Records.Any()) return;

            var columns = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Date", "Date" },
                { "Employee", "Employee" },
                { "Branch", "Branch" },
                { "In", "In" },
                { "Out", "Out" },
                { "Status", "Status" },
                { "Hours", "Hours" },
                { "Overtime", "Overtime" },
                { "Wage", "Wage Cost" }
            };

            var data = Records.Select(r => new 
            {
                Date = r.Date.ToShortDateString(),
                Employee = r.EmployeeName,
                Branch = r.Branch,
                In = r.InTime,
                Out = r.OutTime,
                Status = r.Status,
                Hours = r.HoursWorkedDisplay,
                Overtime = r.OvertimeHoursDisplay,
                Wage = r.WageDisplay
            });
            
            var title = $"Attendance Report: {StartDate:dd MMM} - {EndDate:dd MMM yyyy}";
            var path = await _exportService.GenerateHtmlReportAsync(data, title, columns);
            
            await _exportService.OpenFileAsync(path);
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
        private async Task Initialize()
        {
            await LoadData();
        }

        async partial void OnSelectedRangeChanged(string value)
        {
            SetRange(value);
            await LoadData();
        }



        partial void OnSearchTextChanged(string value) => FilterRecords();
        partial void OnSelectedPayTypeChanged(string value) => FilterRecords();
        partial void OnSelectedBranchChanged(string value) => FilterRecords();

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
                // Note: Branch might be null on record, check carefully.
                // Assuming "Johannesburg" as default if null, or strict check? 
                // Using Loose matching: if filter is JHB, we want JHB records.
                // If record has no branch, maybe exclude? Or assume JHB? 
                // Let's assume explicit check on Branch property.
                string recordBranch = r.Branch ?? "Johannesburg"; // Default per HistoryRecordViewModel logic needed? Or check r.Branch directly?
                // r.Branch property in VM: public string Branch => _attendance.Branch;
                // If null, filter might fail. Let's use string comparison safely.
                bool matchBranch = SelectedBranch == "All" || string.Equals(recordBranch, SelectedBranch, StringComparison.OrdinalIgnoreCase);

                return matchText && matchPay && matchBranch;
            }).ToList();

            var list = new ObservableCollection<HistoryRecordViewModel>(filtered);
            Records = list;
            
            // Recalculate totals for filtered view
            if (filtered.Any())
            {
                TotalWages = filtered.Sum(r => r.Wage);
                TotalHours = filtered.Sum(r => r.HoursWorked);
            }
            else
            {
                 TotalWages = 0;
                 TotalHours = 0;
            }
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
