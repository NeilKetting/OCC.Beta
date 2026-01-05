using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeManagementViewModel : ViewModelBase
    {
        private readonly IRepository<Employee> _staffRepository;

        [ObservableProperty]
        private string _activeTab = "Manage Staff";

        [ObservableProperty]
        private int _totalStaff = 0;

        [ObservableProperty]
        private int _permanentCount = 0;

        [ObservableProperty]
        private int _contractCount = 0;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        partial void OnSearchQueryChanged(string value)
        {
            FilterStaff();
        }

        // Cache for all loaded members
        private System.Collections.Generic.List<Employee> _allStaffMembers = new();

        [ObservableProperty]
        private ObservableCollection<Employee> _staffMembers = new();

        [ObservableProperty]
        private int _selectedFilterIndex = 0;

        partial void OnSelectedFilterIndexChanged(int value)
        {
            FilterStaff();
        }

        private void FilterStaff()
        {
            if (_allStaffMembers == null) return;

            var filtered = _allStaffMembers.AsEnumerable();

            // 1. Text Search
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(s => 
                       (s.FirstName?.ToLower().Contains(query) ?? false) ||
                       (s.LastName?.ToLower().Contains(query) ?? false) ||
                       (s.EmployeeNumber?.ToLower().Contains(query) ?? false)
                );
            }

            // 2. Type Filter
            filtered = _selectedFilterIndex switch
            {
                1 => filtered.Where(s => s.EmploymentType == EmploymentType.Permanent),
                2 => filtered.Where(s => s.EmploymentType == EmploymentType.Contract),
                _ => filtered
            };

            StaffMembers = new ObservableCollection<Employee>(filtered);
        }

        public EmployeeManagementViewModel(IRepository<Employee> staffRepository)
        {
            _staffRepository = staffRepository;
            LoadData(); 
        }

        public async void LoadData()
        {
            try 
            {
                var staff = await _staffRepository.GetAllAsync();
                
                _allStaffMembers = staff.ToList(); // Cache full list
                FilterStaff();
                
                TotalStaff = _allStaffMembers.Count;
                PermanentCount = _allStaffMembers.Count(s => s.EmploymentType == EmploymentType.Permanent);
                ContractCount = _allStaffMembers.Count(s => s.EmploymentType == EmploymentType.Contract);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading staff: {ex.Message}");
            }
        }

        [ObservableProperty]
        private bool _isInvitePopupVisible;

        [ObservableProperty]
        private EmployeeDetailViewModel? _invitePopup;

        [ObservableProperty]
        private Employee? _selectedStaffMember;

        partial void OnSelectedStaffMemberChanged(Employee? value)
        {
            if (value != null)
            {
                EditEmployee(value);
                SelectedStaffMember = null; // Reset selection so we can click again if needed
            }
        }

        [RelayCommand]
        private void InvitePeople()
        {
            InvitePopup = new EmployeeDetailViewModel(_staffRepository);
            InvitePopup.CloseRequested += (s, e) => IsInvitePopupVisible = false;
            InvitePopup.EmployeeAdded += (s, e) => LoadData(); 
            IsInvitePopupVisible = true;
        }

        [RelayCommand]
        public void EditEmployee(Employee staff)
        {
            if (staff == null) return;

            InvitePopup = new EmployeeDetailViewModel(_staffRepository);
            InvitePopup.Load(staff);
            InvitePopup.CloseRequested += (s, e) => IsInvitePopupVisible = false;
            InvitePopup.EmployeeAdded += (s, e) => LoadData(); // Refresh list on save
            IsInvitePopupVisible = true;
        }

        [RelayCommand]
        public async Task DeleteEmployee(Employee staff)
        {
            if (staff == null) return;

            // Optional: Confirm dialog could go here
            await _staffRepository.DeleteAsync(staff.Id);
            LoadData();
        }

        [RelayCommand]
        private void SwitchTab(string tabName)
        {
            ActiveTab = tabName;
        }
    }
}
