using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Messages;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ViewModels.Core;
using System.Collections.Generic;
using Avalonia.Threading;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class TimeLiveViewModel : ViewModelBase, IRecipient<UpdateStatusMessage>, IRecipient<EntityUpdatedMessage>
    {

        #region Private Members

        private readonly ITimeService _timeService;
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;

        #endregion

        #region Observables

        [ObservableProperty]
        private ObservableCollection<LiveUserCardViewModel> _liveUsers = new();

        [ObservableProperty]
        private string _lastUpdatedText = "Updated on " + DateTime.Now.ToString("dd MMMM yyyy HH:mmtt").ToLower();

        [ObservableProperty]
        private bool _isTimesheetVisible = false;

        [ObservableProperty]
        private DailyTimesheetViewModel? _currentTimesheet;

        #endregion

        #region Constructors

        public TimeLiveViewModel()
        {
            // Parameterless constructor for design-time support
            _timeService = null!;
            _authService = null!;
            _serviceProvider = null!;
            _dialogService = null!;
        }

        public TimeLiveViewModel(ITimeService timeService, IAuthService authService, IServiceProvider serviceProvider, IDialogService dialogService)
        {
            _timeService = timeService;
            _authService = authService;
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            
            InitializeCommand.Execute(null);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task Initialize()
        {
            LastUpdatedText = "Updated on " + DateTime.Now.ToString("dd MMMM yyyy HH:mmtt").ToLower();
            await LoadLiveData();
        }

        [RelayCommand]
        private void OpenTimesheet()
        {
            CurrentTimesheet = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<DailyTimesheetViewModel>(_serviceProvider);
            CurrentTimesheet.CloseRequested += (s, e) => CloseTimesheet();
            IsTimesheetVisible = true;
        }

        [RelayCommand]
        private void CloseTimesheet()
        {
            IsTimesheetVisible = false;
            CurrentTimesheet = null;
        }

        [RelayCommand]
        private async Task ClearAttendance()
        {
            try
            {
                await _timeService.ClearAllAttendanceAsync();
                // Trigger refresh via message or direct reload
                await Initialize();
            
                // Send message to notify others?
                 WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "ClearAll", Guid.Empty));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimeLiveViewModel] Error Clearing Attendance: {ex.Message}");
                if (_dialogService != null) await _dialogService.ShowAlertAsync("Error", $"Failed to clear attendance: {ex.Message}");
            }
        }

        #endregion

        #region Methods

        private async Task LoadLiveData()
        {
            try
            {
                var allStaff = await _timeService.GetAllStaffAsync();
                
                // Fetch today's records (for historical 'today' view) AND any active records
                var today = DateTime.Today;
                var todayAttendance = await _timeService.GetDailyAttendanceAsync(today);
                var activeAttendance = await _timeService.GetActiveAttendanceAsync();
                
                var mergedAttendance = todayAttendance.Concat(activeAttendance)
                                                      .Where(x => (x.CheckOutTime == null || x.CheckOutTime == DateTime.MinValue) && x.Date.Date == today)
                                                      .GroupBy(x => x.EmployeeId)
                                                      .Select(g => g.OrderByDescending(r => r.CheckInTime ?? r.Date.Add(r.ClockInTime ?? TimeSpan.Zero)).First())
                                                      .ToList();

                // === NEW: Monthly Hours Calculation ===
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var monthlyRecords = await _timeService.GetAttendanceByRangeAsync(startOfMonth, today);
                
                var userViewModels = new List<LiveUserCardViewModel>();

                // Only show employees that have an ACTIVE attendance record (Live means "Currently Here")
                foreach (var attendance in mergedAttendance)
                {
                    // FILTER: Active Only
                    if (attendance.CheckOutTime != null) continue;

                    var employee = allStaff.FirstOrDefault(e => e.Id == attendance.EmployeeId);
                    if (employee == null) continue;

                    // Determine status
                    bool isPresent = attendance.Status == AttendanceStatus.Present || attendance.Status == AttendanceStatus.Late;
                    TimeSpan? clockIn = attendance.CheckInTime?.TimeOfDay ?? attendance.ClockInTime;
                    TimeSpan? clockOut = attendance.CheckOutTime?.TimeOfDay;

                    string FormatName(string name) => 
                        System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name?.ToLower() ?? "");

                    var vm = new LiveUserCardViewModel
                    {
                        EmployeeId = employee.Id,
                        DisplayName = $"{FormatName(employee.FirstName)}, {FormatName(employee.LastName)}"
                    };
                    vm.SetStatus(isPresent, clockIn, clockOut, employee.Branch ?? "Unknown");

                    // === Monthly Hours ===
                    var empRecords = monthlyRecords.Where(r => r.EmployeeId == employee.Id);
                    double totalHours = 0;
                    foreach (var record in empRecords)
                    {
                         if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
                         {
                             totalHours += (record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours;
                         }
                         else if (record.CheckInTime.HasValue && record.CheckOutTime == null && record.Date.Date == today)
                         {
                             // Currently active session: Count hours so far
                              totalHours += (DateTime.Now - record.CheckInTime.Value).TotalHours;
                         }
                    }
                    vm.TotalMonthHours = totalHours;
                    vm.TotalMonthHoursDisplay = $"{totalHours:F1}h";

                    // === Overtime & Late Logic ===
                    // Requirements:
                    // Weekdays / Normal: 1.5x (Before Start or After End)
                    // Saturday: 1.5x
                    // Sunday / Public Holiday: 2.0x
                    
                    var dow = today.DayOfWeek;
                    bool isWeekend = dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;
                    bool isHoliday = false; // Placeholder for Public Holiday check

                    // 1. Determine multipliers
                    double multiplier = 1.0;
            if (dow == DayOfWeek.Sunday || isHoliday) multiplier = 2.0;
                    else if (dow == DayOfWeek.Saturday) multiplier = 1.5;
                    else multiplier = 1.0;

                    // 2. Define standard window (Employee specific or Branch default)
                    TimeSpan shiftStart = employee.ShiftStartTime ?? new TimeSpan(7, 0, 0);
                    TimeSpan shiftEnd = employee.ShiftEndTime ?? (employee.Branch?.Contains("Cape") == true ? new TimeSpan(17, 0, 0) : new TimeSpan(16, 0, 0));

                    // 3. Apply Overtime Status
                    var currentTime = DateTime.Now.TimeOfDay;
                    bool isOutsideNormalHours = currentTime < shiftStart || currentTime > shiftEnd;

                    if (isOutsideNormalHours && multiplier < 1.5)
                    {
                        multiplier = 1.5;
                    }

                    if (multiplier > 1.0)
                    {
                        vm.IsOvertimeActive = true;
                        vm.OvertimeText = $"OVERTIME {multiplier:F1}x";
                        vm.OvertimeColor = multiplier >= 2.0 ? Avalonia.Media.Brushes.Red : Avalonia.Media.SolidColorBrush.Parse("#F97316"); // Orange-500
                    }

                    // 4. Apply Late Status
                    // Requirements: If clock-in > 30 minutes after ShiftStartTime
                    if (clockIn.HasValue && clockIn.Value > shiftStart.Add(TimeSpan.FromMinutes(30)))
                    {
                        vm.IsLate = true;
                        vm.LateText = "LATE";
                    }

                    userViewModels.Add(vm);
                }

                Dispatcher.UIThread.Post(() =>
                {
                    LiveUsers.Clear();
                    foreach (var u in userViewModels.OrderBy(x => x.DisplayName))
                    {
                        LiveUsers.Add(u);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading live data: {ex.Message}");
                if (_dialogService != null)
                {
                    // Use Dispatcher to ensure UI thread if being called from background messager
                    await Dispatcher.UIThread.InvokeAsync(async () => 
                        // Don't await inside InvokeAsync lambda for the dialog result if we don't need it, 
                        // but actually we should warn user.
                        await _dialogService.ShowAlertAsync("Error", $"Critical Error loading live timesheet: {ex.Message}")
                    );
                }
            }
        }

        #endregion

        public void Receive(UpdateStatusMessage message)
        {
            if (message.Value == "Attendance Saved")
            {
                _ = Initialize();
            }
        }

        public void Receive(EntityUpdatedMessage message)
        {
            if (message.Value.EntityType == "AttendanceRecord")
            {
                // Real-time update from any client
                Dispatcher.UIThread.InvokeAsync(Initialize);
            }
        }

    }
}
