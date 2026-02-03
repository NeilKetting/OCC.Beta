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
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.Time
{
    public partial class DailyTimesheetViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;
        private readonly ILeaveService _leaveService;
        private readonly IRepository<OvertimeRequest> _overtimeRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;

        #region Observables

        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _pendingStaff = new();

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _loggedStaff = new();



        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedBranch = "Johannesburg";
        public string[] Branches => new[] { "All", "Johannesburg", "Cape Town" };

        [ObservableProperty]
        private bool _isManualMode;
        
        [ObservableProperty]
        private bool _isAllSelected = true;

        [ObservableProperty]
        private bool _isAllLoggedSelected = true;

        partial void OnIsAllSelectedChanged(bool value)
        {
            foreach (var staff in PendingStaff)
            {
                staff.IsSelected = value;
            }
        }

        partial void OnIsAllLoggedSelectedChanged(bool value)
        {
            foreach (var staff in LoggedStaff)
            {
                staff.IsSelected = value;
            }
        }

        #endregion

        private System.Collections.Generic.List<StaffAttendanceViewModel> _allPendingCache = new();
        private System.Collections.Generic.List<StaffAttendanceViewModel> _allLoggedCache = new();

        public event EventHandler? CloseRequested;

        public DailyTimesheetViewModel(
            ITimeService timeService, 
            ILeaveService leaveService,
            IRepository<OvertimeRequest> overtimeRepository, // NEW
            IDialogService dialogService,
            IAuthService authService)
        {
            _timeService = timeService;
            _leaveService = leaveService;
            _overtimeRepository = overtimeRepository;
            _dialogService = dialogService;
            _authService = authService;
            
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                // 1. Fetch Staff, Attendance, Leaves
                var allStaff = await _timeService.GetAllStaffAsync();
                var todayRecords = await _timeService.GetDailyAttendanceAsync(Date);
                var activeRecords = await _timeService.GetActiveAttendanceAsync(); // NEW: Check ALL active, not just today's
                var approvedLeave = await _leaveService.GetApprovedRequestsForDateAsync(Date);

                _allPendingCache.Clear();
                _allLoggedCache.Clear();

                foreach (var emp in allStaff.Where(e => e.Status == EmployeeStatus.Active).OrderBy(e => e.FirstName))
                {
                    // Leave Mapping
                    var leave = approvedLeave.FirstOrDefault(l => l.EmployeeId == emp.Id);
                    
                    // NEW LOGIC: Filter out if they have ANY ACTIVE shift (from today OR yesterday)
                    // We check activeRecords (which lists anyone with CheckOutTime == null)
                    var isActive = activeRecords.Any(r => r.EmployeeId == emp.Id);
                    
                    // ALSO filter out if they have a *completed* record for TODAY (so they don't appear in Pending if they finished a shift today)
                    // Wait, previous logic was: "If currently clocked out, appear in Pending".
                    // So we ONLY care about isActive.
                    
                    // Re-read requirement: "Employee clocked out... should lie on the left".
                    // So:
                    // - Is Active? -> NOT Pending (they are working).
                    // - Not Active? -> Pending (Ready to work).
                    
                    if (!isActive)
                    {
                        var vm = new StaffAttendanceViewModel(emp);
                        if (leave != null)
                        {
                            vm.IsOnLeave = true;
                            vm.LeaveType = leave.LeaveType.ToString();
                        }
                        _allPendingCache.Add(vm);
                    }
                }

                // 3. Populate Logged (All records, supporting multiple per employee)
                // We iterate records instead of staff to allow multiple rows
                
                // MERGE: If viewing Today, include ALL active records (even from yesterday)
                // This fixes the bug where "Overtime" staff from yesterday don't appear in the list to be clocked out.
                var recordsToProcess = todayRecords.ToList();
                
                if (Date.Date >= DateTime.Today)
                {
                    foreach (var active in activeRecords)
                    {
                        if (!recordsToProcess.Any(r => r.Id == active.Id))
                        {
                            recordsToProcess.Add(active);
                        }
                    }
                }

                // SORT: Descending by CheckInTime (Full DateTime) so newest is top
                foreach (var record in recordsToProcess.OrderByDescending(r => r.CheckInTime))
                {
                    var emp = allStaff.FirstOrDefault(e => e.Id == record.EmployeeId);
                    if (emp == null) continue; // Should not happen

                    var vm = new StaffAttendanceViewModel(emp);
                    vm.Id = record.Id;
                    vm.Status = record.Status;
                    
                    // Map times
                    if (record.ClockInTime.HasValue) 
                        vm.ClockInTime = record.ClockInTime; 
                    else if (record.CheckInTime != DateTime.MinValue && record.CheckInTime != null) 
                        vm.ClockInTime = record.CheckInTime.Value.TimeOfDay;
                    else 
                        vm.ClockInTime = null;
                    
                    if (record.CheckOutTime != null && record.CheckOutTime != DateTime.MinValue)
                        vm.ClockOutTime = record.CheckOutTime.Value.TimeOfDay;
                    else
                        vm.ClockOutTime = null; // Still clocked in

                    _allLoggedCache.Add(vm);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in LoadDataAsync: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to load timesheet: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ReClockIn(StaffAttendanceViewModel item)
        {
             if (item == null || _isProcessingAction) return;
             _isProcessingAction = true;
             try
             {
                 // 1. Prevent Multiple Active Shifts
                 // Check if this employee ALREADY has an active record (CheckOutTime is null) - checking GLOBAL active list to catch yesterday's shifts
                 var activeRecords = await _timeService.GetActiveAttendanceAsync();
                 bool hasActiveShift = activeRecords.Any(x => x.EmployeeId == item.EmployeeId);
                 
                 if (hasActiveShift)
                 {
                     await _dialogService.ShowAlertAsync("Active Shift Exists", $"{item.Name} is already clocked in. Please clock them out before starting a new shift.");
                     return;
                 }

                 // Logic: Create a BRAND NEW record for this employee.
                 // We can fetch the employee details from the viewmodel item.
                 var emp = await _timeService.GetAllStaffAsync(); 
                 var staff = emp.FirstOrDefault(e => e.Id == item.EmployeeId);
                 
                 if (staff == null) return;

                 IsSaving = true;
                 try
                 {
                     var now = DateTime.Now;
                     var record = new AttendanceRecord
                     {
                         Id = Guid.NewGuid(),
                         EmployeeId = staff.Id,
                         Date = Date,
                         Branch = staff.Branch,
                         Status = AttendanceStatus.Present,
                         CheckInTime = now,
                         ClockInTime = now.TimeOfDay,
                         CheckOutTime = null, // Open shift
                         CachedHourlyRate = (decimal?)staff.HourlyRate // SNAPSHOT RATE
                     };

                     await _timeService.SaveAttendanceRecordAsync(record);

                     // Create a new VM for this new record
                     var newVm = new StaffAttendanceViewModel(staff)
                     {
                         Id = record.Id,
                         Status = AttendanceStatus.Present,
                         ClockInTime = now.TimeOfDay,
                         ClockOutTime = null
                     };

                     // Add to cache (at TOP) and refresh
                     _allLoggedCache.Insert(0, newVm); // Insert at 0 to keep "Newest Top" order consistent
                     ApplyFilters();
                     
                     WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{staff.FirstName} started a new shift."));
                 }
                 catch (Exception ex)
                 {
                     System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in ReClockIn: {ex.Message}");
                     if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to re-clock in: {ex.Message}");
                 }
                 finally { IsSaving = false; }
             }
             finally
             {
                 _isProcessingAction = false;
             }
        }

        private void ApplyFilters()
        {
             var query = SearchText?.Trim();
             var branch = SelectedBranch;

             // Helper filter function
             bool Filter(StaffAttendanceViewModel s)
             {
                 bool matchBranch = branch == "All" || string.Equals(s.Branch?.Trim(), branch, StringComparison.OrdinalIgnoreCase);
                 bool matchSearch = string.IsNullOrWhiteSpace(query) || (s.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
                 return matchBranch && matchSearch;
             }

             PendingStaff = new ObservableCollection<StaffAttendanceViewModel>(
                 _allPendingCache.Where(Filter)
                                 .OrderBy(x => x.IsOnLeave)
                                 .ThenBy(x => x.Name));
             
             // Ensure LoggedStaff maintains the cache order (which we set to Descending)
             LoggedStaff = new ObservableCollection<StaffAttendanceViewModel>(_allLoggedCache.Where(Filter));
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnSelectedBranchChanged(string value) => ApplyFilters();

        private bool _isProcessingAction = false;

        [RelayCommand]
        private async Task MarkPresent(StaffAttendanceViewModel item)
        {
            if (item == null || _isProcessingAction) return;
            _isProcessingAction = true;
            try
            {
                // CONFLICT CHECK: Is on Leave?
                if (item.IsOnLeave)
                {
                    var confirm = await _dialogService.ShowConfirmationAsync(
                        "Leave Conflict", 
                        $"{item.Name} is on Approved {item.LeaveType} Leave today.\n\nDo you want to CANCEL their leave and mark them Present?");
                    
                    if (!confirm) return;
                }

                // CONFLICT CHECK: Global Active
                var activeRecords = await _timeService.GetActiveAttendanceAsync();
                if (activeRecords.Any(r => r.EmployeeId == item.EmployeeId))
                {
                    await _dialogService.ShowAlertAsync("Already Clocked In", $"{item.Name} has an active shift (possibly from yesterday). Please clock them out first.");
                    return;
                }

                DateTime checkInTime = DateTime.Now;
                DateTime? checkOutTime = null;

                if (IsManualMode)
                {
                    // Default Start Time: Shift Start or 07:00:00
                    TimeSpan defaultStart = item.Staff.ShiftStartTime ?? new TimeSpan(7, 0, 0);

                    // Open Dialog to get Manual Times (IN ONLY)
                    var result = await _dialogService.ShowEditAttendanceAsync(defaultStart, null, showIn: true, showOut: false);
                    if (!result.Confirmed) return; // User cancelled
                    
                    if (result.InTime.HasValue)
                    {
                        checkInTime = Date.Date.Add(result.InTime.Value); 
                    }
                }

                IsSaving = true;
                try
                {
                    var record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = item.EmployeeId,
                        Date = Date,
                        Branch = item.Branch,
                        Status = AttendanceStatus.Present,
                        CheckInTime = checkInTime,
                        ClockInTime = checkInTime.TimeOfDay,
                        CheckOutTime = checkOutTime, 
                        CachedHourlyRate = (decimal?)item.Staff.HourlyRate 
                    };

                    await _timeService.SaveAttendanceRecordAsync(record);
                    
                    item.Id = record.Id;
                    item.Status = AttendanceStatus.Present;
                    item.ClockInTime = record.ClockInTime;
                    item.ClockOutTime = record.CheckOutTime?.TimeOfDay;

                    MoveToLogged(item);
                    WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{item.Name} marked Present"));
                }
                catch (Exception ex)
                {
                     System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in MarkPresent: {ex.Message}");
                     if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to mark present: {ex.Message}");
                }
                finally { IsSaving = false; }
            }
            finally
            {
                _isProcessingAction = false;
            }
        }

        [RelayCommand]
        private async Task ClockInSelected()
        {
            var toClockIn = PendingStaff.Where(s => s.IsSelected).ToList();
            if (!toClockIn.Any())
            {
                await _dialogService.ShowAlertAsync("Selection Required", "Please select at least one employee to clock in.");
                return;
            }

            DateTime checkInTime = DateTime.Now;

            if (IsManualMode)
            {
                // Typical start time (use first item or default)
                var first = toClockIn.First();
                TimeSpan defaultStart = first.Staff.ShiftStartTime ?? new TimeSpan(7, 0, 0);

                var result = await _dialogService.ShowEditAttendanceAsync(defaultStart, null, showIn: true, showOut: false);
                if (!result.Confirmed || !result.InTime.HasValue) return;
                
                checkInTime = Date.Date.Add(result.InTime.Value);
            }
            else
            {
                var confirm = await _dialogService.ShowConfirmationAsync("Bulk Clock In", $"Clock in {toClockIn.Count} employees as Present?");
                if (!confirm) return;
            }

            IsSaving = true;
            try
            {
                int count = 0;
                foreach (var item in toClockIn)
                {
                    if (item.IsOnLeave) continue; 

                    var record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = item.EmployeeId,
                        Date = Date,
                        Branch = item.Branch,
                        Status = AttendanceStatus.Present,
                        CheckInTime = checkInTime,
                        ClockInTime = checkInTime.TimeOfDay,
                        CheckOutTime = null, 
                        CachedHourlyRate = (decimal?)item.Staff.HourlyRate 
                    };

                    await _timeService.SaveAttendanceRecordAsync(record);
                    
                    item.Id = record.Id;
                    item.Status = AttendanceStatus.Present;
                    item.ClockInTime = record.ClockInTime;
                    item.ClockOutTime = null;
                    count++;
                }

                foreach (var item in toClockIn.Where(x => x.Id != Guid.Empty))
                {
                    MoveToLogged(item);
                }

                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{count} staff members clocked in."));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Bulk clock-in failed: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task ClockOutSelected()
        {
            // Only clock out staff who are currently active (CheckOutTime is null)
            var toClockOut = LoggedStaff.Where(s => s.IsSelected && s.ClockOutTime == null).ToList();
            if (!toClockOut.Any())
            {
                await _dialogService.ShowAlertAsync("Selection Required", "Please select at least one active employee to clock out.");
                return;
            }

            DateTime checkOutTime = DateTime.Now;

            if (IsManualMode)
            {
                // Typical end time
                // Typical end time - use first person's scheduled end
                var first = toClockOut.First();
                TimeSpan defaultEnd = first.Staff?.ShiftEndTime ?? new TimeSpan(16, 45, 0);

                var result = await _dialogService.ShowEditAttendanceAsync(null, defaultEnd, showIn: false, showOut: true);
                if (!result.Confirmed || !result.OutTime.HasValue) return;
                
                checkOutTime = Date.Date.Add(result.OutTime.Value);
            }
            else
            {
                var confirm = await _dialogService.ShowConfirmationAsync("Bulk Clock Out", $"Clock out {toClockOut.Count} employees?");
                if (!confirm) return;
            }

            IsSaving = true;
            try
            {
                int count = 0;
                foreach (var item in toClockOut)
                {
                    var record = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                    if (record != null)
                    {
                        record.CheckOutTime = checkOutTime;
                        await _timeService.SaveAttendanceRecordAsync(record);
                        
                        item.ClockOutTime = checkOutTime.TimeOfDay;
                        item.Status = record.Status;
                        count++;
                    }
                }

                // Refresh the items in the collection
                foreach (var item in toClockOut)
                {
                    MoveToPending(item);
                }

                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{count} staff members clocked out."));
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Bulk clock-out failed: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task MarkAbsent(StaffAttendanceViewModel item)
        {
            if (item == null || _isProcessingAction) return;
            _isProcessingAction = true;
            try
            {
                IsSaving = true;
                try
                {
                    // STRICT DATA INTEGRITY: Absent = No Times
                    var record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = item.EmployeeId,
                        Date = Date,
                        Branch = item.Branch,
                        Status = AttendanceStatus.Absent,
                        CheckInTime = null,
                        ClockInTime = null,
                        CheckOutTime = Date // Closed immediately so it doesn't show as "Live"
                    };

                    await _timeService.SaveAttendanceRecordAsync(record);
                    
                    item.Id = record.Id;
                    item.Status = AttendanceStatus.Absent;
                    item.Id = record.Id;
                    item.Status = AttendanceStatus.Absent;
                    item.ClockInTime = null;
                    item.ClockOutTime = Date.TimeOfDay; // Or just ensure it has a value so it's not "active"

                    MoveToLogged(item);
                    WeakReferenceMessenger.Default.Send(new UpdateStatusMessage($"{item.Name} marked Absent"));
                }
                catch (Exception ex)
                {
                     System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in MarkAbsent: {ex.Message}");
                     if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to mark absent: {ex.Message}");
                }
                finally { IsSaving = false; }
            }
            finally
            {
                _isProcessingAction = false;
            }
        }

        [RelayCommand]
        private async Task ClockOut(StaffAttendanceViewModel item)
        {
            if (item == null || item.Id == Guid.Empty || _isProcessingAction) return;
            _isProcessingAction = true;
            try
            {
                // 1. Determine Business Hours / Expected End Time
                // Priority:
                // A. Approved Overtime End Time
                // B. Employee Specific Shift End Time
                // C. Branch Default (Fallback)

                TimeSpan expectedEndTime;

                // B. Employee Shift
                // We need to fetch the employee for this info, waiting on item.Staff might be cached snapshot but should be mostly fine.
                // For rigorous check we can fetch from Repo but item.Staff is decent.
                if (item.Staff != null && item.Staff.ShiftEndTime.HasValue)
                {
                    expectedEndTime = item.Staff.ShiftEndTime.Value;
                }
                else
                {
                    // C. Branch Default
                    // JHB: 16:00, CPT: 17:00
                    // JHB: 16:45, CPT: 16:45
                    string branch = item.Branch ?? "Johannesburg";
                    expectedEndTime = new TimeSpan(16, 45, 0);
                }

                // A. Overtime Check
                // Find APPROVED overtime for this user on THIS DATE (The date of the attendance record, not necessarily today if clocking out late)
                // But we typically clock out "now".
                // Logic: If I have approved overtime until 20:00, and I leave at 18:00, I am leaving early.
                
                try 
                {
                    var allOvertime = await _overtimeRepository.GetAllAsync();
                    var approvedOt = allOvertime.FirstOrDefault(o => 
                        o.EmployeeId == item.EmployeeId && 
                        o.Date.Date == Date.Date && // Match sheet date
                        o.Status == LeaveStatus.Approved); // MUST be Approved

                    if (approvedOt != null)
                    {
                        // If OT extends beyond normal shift, use OT end time.
                        if (approvedOt.EndTime > expectedEndTime)
                        {
                            expectedEndTime = approvedOt.EndTime;
                        }
                    }
                }
                catch (Exception ex)
                {
                   System.Diagnostics.Debug.WriteLine($"[ClockOut] Error fetching overtime: {ex.Message}");
                }

                var now = DateTime.Now;
                string? leaveReason = null;
                string? leaveNote = null;

                // Check if Early
                bool isSameDay = now.Date == Date.Date;
                bool isWithinCommittedHours = false;

                // Define Normal Window
                TimeSpan shiftStart;
                if (item.Staff != null && item.Staff.ShiftStartTime.HasValue) shiftStart = item.Staff.ShiftStartTime.Value;
                else shiftStart = new TimeSpan(7, 0, 0);



                // Check if Manual Mode for Clock Out
                if (IsManualMode)
                {
                    // JHB: 16:45, CPT: 16:30
                    string branch = item.Branch ?? "Johannesburg";
                    TimeSpan defaultEnd = branch.Contains("Cape", StringComparison.OrdinalIgnoreCase) 
                        ? new TimeSpan(16, 30, 0) 
                        : new TimeSpan(16, 45, 0);

                     // Open Dialog to get Manual Times (OUT ONLY)
                     // Pass defaultEnd as 'currentOut' so it populates the picker
                    var result = await _dialogService.ShowEditAttendanceAsync(null, defaultEnd, showIn: false, showOut: true);
                    if (!result.Confirmed) return;
                    
                    if (result.OutTime.HasValue)
                    {
                         // Override 'now' with manual time
                         now = Date.Date.Add(result.OutTime.Value);
                    }
                    else
                    {
                        // User confirmed but cleared time? Default to now.
                         now = DateTime.Now;
                    }
                }
                else 
                {
                    // Only do Early Leave check if Not Manual (Manual assumes user knows what they are doing)
                    // We are 'within' committed hours if Now is between ShiftStart and ExpectedEndTime
                    if (now.TimeOfDay >= shiftStart && now.TimeOfDay < expectedEndTime)
                    {
                        isWithinCommittedHours = true;
                    }

                    if (isSameDay && isWithinCommittedHours)
                    {
                        var diff = expectedEndTime - now.TimeOfDay;
                        if (diff.TotalMinutes > 15) // 15 min buffer
                        {
                            var result = await _dialogService.ShowLeaveEarlyReasonAsync();
                            if (!result.Confirmed) return;

                            leaveReason = result.Reason;
                            leaveNote = result.Note;
                        }
                    }
                }
                // If clocking out on a FUTURE date (e.g. next morning), we assume they worked full shift + more, so no "Early" prompt.

                IsSaving = true;
                try 
                {
                    var record = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                    if (record != null)
                    {
                        record.CheckOutTime = now;
                        // If we have a reason, update status and notes
                        if (!string.IsNullOrEmpty(leaveReason))
                        {
                            record.Status = AttendanceStatus.LeaveEarly;
                            record.LeaveReason = leaveReason;
                            record.Notes = !string.IsNullOrEmpty(leaveNote) ? $"[Leave Early Note] {leaveNote}" : null;
                        }
                        else
                        {
                            // Logic: If status was already something else (e.g. Present), keep it. 
                            // If they worked full shift, it stays Present.
                            // If they arrived Late, it stays Late.
                        }
                        
                        await _timeService.SaveAttendanceRecordAsync(record);
                        
                        item.ClockOutTime = record.CheckOutTime.Value.TimeOfDay;
                        // status might update if 'LeaveEarly' was set
                        item.Status = record.Status; 
                        
                        // Trigger UI refresh?
                        var loggedItem = LoggedStaff.FirstOrDefault(x => x.Id == item.Id);
                        if (loggedItem != null) 
                        {
                            loggedItem.ClockOutTime = item.ClockOutTime;
                            loggedItem.Status = item.Status;
                        }
                        // Move back to Pending
                        MoveToPending(item);
                    }
                }
                catch (Exception ex)
                {
                     System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in ClockOut: {ex.Message}");
                     if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to clock out: {ex.Message}");
                }
                finally { IsSaving = false; }
            }
            finally
            {
                _isProcessingAction = false;
            }
        }

        [RelayCommand]
        private async Task EditEntry(StaffAttendanceViewModel item)
        {
            if (item == null || item.Id == Guid.Empty) return;

            // Open Dialog
            var result = await _dialogService.ShowEditAttendanceAsync(item.ClockInTime, item.ClockOutTime);
            if (!result.Confirmed) return;

            IsSaving = true;
            try
            {
                var record = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                if (record != null)
                {
                    // Update times
                    if (result.InTime.HasValue)
                    {
                        record.CheckInTime = Date.Add(result.InTime.Value); 
                        record.ClockInTime = result.InTime;
                        // Status logic: If correcting to a time, ensure Present/Late/etc.
                        // Ideally we re-evaluate status, but manual override implies Present usually.
                        if (record.Status == AttendanceStatus.Absent) record.Status = AttendanceStatus.Present;
                    }
                    
                    if (result.OutTime.HasValue)
                    {
                        record.CheckOutTime = Date.Add(result.OutTime.Value);
                    }
                    else
                    {
                        record.CheckOutTime = null;
                    }
                    
                    await _timeService.SaveAttendanceRecordAsync(record);

                    // Update UI
                    item.ClockInTime = result.InTime;
                    item.ClockOutTime = result.OutTime;
                    item.Status = record.Status;

                    // Trigger UI refresh
                    var loggedItem = LoggedStaff.FirstOrDefault(x => x.Id == item.Id);
                    if (loggedItem != null)
                    {
                        loggedItem.ClockInTime = item.ClockInTime;
                        loggedItem.ClockOutTime = item.ClockOutTime;
                        loggedItem.Status = item.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[DailyTimesheetViewModel] Error in EditEntry: {ex.Message}");
                 if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to edit entry: {ex.Message}");
            }
            finally { IsSaving = false; }
        }
        
        [RelayCommand]
        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MoveToLogged(StaffAttendanceViewModel item)
        {
            // Use RemoveAll for safety to ensure it's gone even if references differ
            _allPendingCache.RemoveAll(x => x.EmployeeId == item.EmployeeId);
            
            // Insert at 0 to match "Newest First" view
            _allLoggedCache.Insert(0, item); 
            
            // Re-apply filter
            ApplyFilters(); 
        }

        private void MoveToPending(StaffAttendanceViewModel item)
        {
            // When clocking out, we want this employee to appear in the Pending list again
            // so they can be clocked in for a new shift if needed.
            
            // Safety check: is he already in pending?
            if (_allPendingCache.Any(x => x.EmployeeId == item.EmployeeId)) return;
            
            // Create a fresh VM for the Pending list
            var pendingVm = new StaffAttendanceViewModel(item.Staff)
            {
                // Reset fields for "New Shift" state
                ClockInTime = null,
                ClockOutTime = null,
                Status = AttendanceStatus.Present
            };
            
            _allPendingCache.Add(pendingVm);
            
            // Re-apply filter
            ApplyFilters(); 
        }
    }
}
