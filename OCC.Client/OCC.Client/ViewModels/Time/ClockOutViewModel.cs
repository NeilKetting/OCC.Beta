using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OCC.Client.Services.Interfaces;
using OCC.Client.ViewModels.Core;
using OCC.Client.ViewModels.Messages;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace OCC.Client.ViewModels.Time
{
    public partial class ClockOutViewModel : ViewModelBase
    {
        private readonly ITimeService _timeService;

        #region Observables

        [ObservableProperty]
        private ObservableCollection<StaffAttendanceViewModel> _staffList = new();

        [ObservableProperty]
        private ObservableCollection<string> _branches = new();

        [ObservableProperty]
        private string _selectedBranch = "All"; // Default to All

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private string _searchText = string.Empty;

        #endregion

        private System.Collections.Generic.List<StaffAttendanceViewModel> _allLoadedStaff = new();
        private readonly DispatcherTimer _timer;

        public ClockOutViewModel(ITimeService timeService)
        {
            _timeService = timeService;
            
            // Timer for Live Updates
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _timer.Tick += (s, e) => 
            {
                 foreach (var item in StaffList)
                 {
                     item.Refresh();
                 }
            };
            _timer.Start();
            
            InitializeCommand.Execute(null);
        }

        [RelayCommand]
        private async Task Initialize()
        {
            IsLoading = true;
            try
            {
                await LoadDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            var date = DateTime.Today;
            var allStaff = await _timeService.GetAllStaffAsync();
            
            // 1. Populate Branches
            var branches = allStaff.Select(s => s.Branch ?? "Unassigned").Distinct().OrderBy(b => b).ToList();
            branches.Insert(0, "All");
            
            if (Branches.Count != branches.Count || !Branches.SequenceEqual(branches))
            {
                Branches = new ObservableCollection<string>(branches);
            }
             
            if (!Branches.Contains(SelectedBranch)) SelectedBranch = "All";

            // 2. Build Cache of Display Models
            _allLoadedStaff.Clear();

            // Use GetActiveAttendanceAsync to ensure we catch overnight/forgotten clock-ins
            var activeRecords = await _timeService.GetActiveAttendanceAsync();

            foreach (var record in activeRecords)
            {
                var staff = allStaff.FirstOrDefault(e => e.Id == record.EmployeeId);
                if (staff == null) continue;

                var vm = new StaffAttendanceViewModel(staff)
                {
                    Id = record.Id,
                    Status = record.Status,
                    ClockInTime = record.CheckInTime?.TimeOfDay ?? record.ClockInTime,
                    Branch = staff.Branch ?? "Unassigned"
                };
                _allLoadedStaff.Add(vm);
            }

            // Sort by Name
            _allLoadedStaff = _allLoadedStaff.OrderBy(s => s.Name).ToList();

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allLoadedStaff == null) return;

            var filtered = _allLoadedStaff.AsEnumerable();
            
            // Branch Filter
            if (SelectedBranch != "All")
            {
                filtered = filtered.Where(s => s.Branch == SelectedBranch);
            }
            
            // Search Filter
            var query = SearchText?.Trim();
            if (!string.IsNullOrWhiteSpace(query))
            {
                 filtered = filtered.Where(s => s.Name != null && s.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            StaffList = new ObservableCollection<StaffAttendanceViewModel>(filtered);
        }

        // Re-load when filter changes
        partial void OnSelectedBranchChanged(string value) => ApplyFilter();
        partial void OnSearchTextChanged(string value) => ApplyFilter();

        [RelayCommand]
        private async Task ClockOut(StaffAttendanceViewModel item)
        {
            if (item == null) return;
            if (string.IsNullOrWhiteSpace(item.LeaveReason))
            {
                 // TODO: Show notification "Reason Required"
                 return;
            }

            IsSaving = true;
            try
            {
                // Fetch the ACTUAL record from DB to ensure we preserve CheckInTime
                var existing = await _timeService.GetAttendanceRecordByIdAsync(item.Id);
                
                if (existing != null)
                {
                    existing.CheckOutTime = DateTime.Now;
                    existing.Status = AttendanceStatus.LeaveEarly; // Or based on time? Defaulting to LeaveEarly as per previous logic
                    existing.LeaveReason = item.LeaveReason;
                    await _timeService.SaveAttendanceRecordAsync(existing);
                }
                else
                {
                    // Fallback (Should typically not happen if ID maps correctly)
                    var record = new AttendanceRecord
                    {
                        Id = item.Id,
                        EmployeeId = item.EmployeeId,
                        Date = DateTime.Today,
                        Status = AttendanceStatus.LeaveEarly,
                        CheckOutTime = DateTime.Now,
                        LeaveReason = item.LeaveReason,
                        Branch = item.Branch
                    };
                     await _timeService.SaveAttendanceRecordAsync(record);
                }

                WeakReferenceMessenger.Default.Send(new UpdateStatusMessage("Clocked Out"));
                WeakReferenceMessenger.Default.Send(new EntityUpdatedMessage("AttendanceRecord", "Updated", item.Id));

                StaffList.Remove(item);
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
