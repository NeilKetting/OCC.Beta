using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.ModelWrappers;
using OCC.Client.ViewModels.Core;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeDetailViewModel : ViewModelBase
    {
        #region Private Members

        private readonly IRepository<Employee> _staffRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IDialogService _dialogService;
        private readonly IAuthService _authService;
        private readonly ILeaveService _leaveService;
        private Guid? _existingStaffId;
        private DateTime _calculatedDoB = DateTime.Now.AddYears(-30);


        #endregion

        #region Events

        public event EventHandler? CloseRequested;
        public event EventHandler? EmployeeAdded;

        #endregion

        #region Observables

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private EmployeeWrapper _wrapper = null!;

        [ObservableProperty]
        private string _saveButtonText = "Add Employee";

        [ObservableProperty]
        private List<string> _availableAccountTypes = new() { "Select Account Type", "Savings", "Cheque", "Transmission" };

        [ObservableProperty]
        private double _currentAnnualLeaveBalance;

        [ObservableProperty]
        private string _sickLeaveCycleEndDisplay = "N/A";

        [ObservableProperty]
        private List<User> _availableUsers = new();

        [ObservableProperty]
        private string? _currentPermissions;

        [ObservableProperty]
        private bool _isPermissionsPopupVisible;

        [ObservableProperty]
        private EmployeePermissionsViewModel? _permissionsPopupVM;

        [ObservableProperty]
        private bool _showPermissionsButton;

        public bool HasLinkedUser => Wrapper.LinkedUserId.HasValue;

        [ObservableProperty]
        private string _leaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";

        [ObservableProperty]
        private BankName _selectedBank = OCC.Shared.Models.BankName.None;

        [ObservableProperty]
        private string _customBankName = string.Empty;

        public bool IsOtherBankSelected => SelectedBank == BankName.Other;

        public BankName[] AvailableBanks { get; } = Enum.GetValues<BankName>();

        partial void OnSelectedBankChanged(BankName value) => OnPropertyChanged(nameof(IsOtherBankSelected));

        #endregion

        #region Properties

        public bool IsHourly
        {
            get => Wrapper.RateType == RateType.Hourly;
            set
            {
                if (value) Wrapper.RateType = RateType.Hourly;
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
            }
        }

        public bool IsSalary
        {
            get => Wrapper.RateType == RateType.MonthlySalary;
            set
            {
                if (value) Wrapper.RateType = RateType.MonthlySalary;
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
            }
        }
        
        public bool IsRsaId
        {
            get => Wrapper.IdType == IdType.RSAId;
            set
            {
                if (value) Wrapper.IdType = IdType.RSAId;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPassport
        {
            get => Wrapper.IdType == IdType.Passport;
            set
            {
                if (value) Wrapper.IdType = IdType.Passport;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPermanent
        {
            get => Wrapper.EmploymentType == EmploymentType.Permanent;
            set
            {
                if (value) Wrapper.EmploymentType = EmploymentType.Permanent;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContract
        {
            get => Wrapper.EmploymentType == EmploymentType.Contract;
            set
            {
                if (value) Wrapper.EmploymentType = EmploymentType.Contract;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContractVisible => IsContract;

        // Exposed Enum Values for ComboBox
        public EmployeeRole[] EmployeeRoles { get; } = Enum.GetValues<EmployeeRole>();

        public List<string> Branches { get; } = new() { "Johannesburg", "Cape Town" };

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Wrapper.FirstName) && string.IsNullOrWhiteSpace(Wrapper.LastName))
                {
                    return _existingStaffId.HasValue ? "Edit Employee" : "New Employee";
                }
                return $"{Wrapper.FirstName}, {Wrapper.LastName}".Trim();
            }
        }

        #endregion

        #region Constructors

        public EmployeeDetailViewModel(IRepository<Employee> staffRepository, IRepository<User> userRepository, IDialogService dialogService, IAuthService authService, ILeaveService leaveService)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _dialogService = dialogService;
            _authService = authService;
            _leaveService = leaveService;
            
            // Ensure the initial wrapper is set via the property so OnWrapperChanged is called
            Wrapper = new EmployeeWrapper(new Employee());
            
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                AvailableUsers = users.OrderBy(u => u.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        public EmployeeDetailViewModel() 
        {
            // _staffRepository and _dialogService will be null, handle in Save
            _staffRepository = null!;
            _userRepository = null!;
            _dialogService = null!;
            _authService = null!;
            _leaveService = null!;
        }

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanSave))]
        private async Task Save()
        {
            // 1. Duplicate Checks
            var allStaff = await _staffRepository.GetAllAsync();
            var otherStaff = _existingStaffId.HasValue 
                ? allStaff.Where(s => s.Id != _existingStaffId.Value).ToList() 
                : allStaff.ToList();

            if (!string.IsNullOrWhiteSpace(Wrapper.IdNumber) && otherStaff.Any(s => s.IdNumber == Wrapper.IdNumber))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with ID Number '{Wrapper.IdNumber}' already exists.");
                 return;
            }

            if (!string.IsNullOrWhiteSpace(Wrapper.EmployeeNumber) && otherStaff.Any(s => s.EmployeeNumber == Wrapper.EmployeeNumber))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with Number '{Wrapper.EmployeeNumber}' already exists.");
                 return;
            }

            if (!string.IsNullOrWhiteSpace(Wrapper.Email) && otherStaff.Any(s => s.Email != null && s.Email.Equals(Wrapper.Email, StringComparison.OrdinalIgnoreCase)))
            {
                 await _dialogService.ShowAlertAsync("Validation Error", $"An employee with Email '{Wrapper.Email}' already exists.");
                 return;
            }

            // 2. Sync UI-Only properties to the Wrapper/Model
            Wrapper.DoB = _calculatedDoB;
            
            // Sync Banking details from ViewModel properties to Wrapper
            if (SelectedBank == OCC.Shared.Models.BankName.None)
            {
                 Wrapper.BankName = null;
            }
            else
            {
                 Wrapper.BankName = IsOtherBankSelected ? CustomBankName : GetEnumDescription(SelectedBank);
            }

            // Sync Account Type
            Wrapper.AccountType = (Wrapper.AccountType == "Select Account Type") ? null : Wrapper.AccountType;

            // 3. Commit ALL changes from Wrapper to the underlying Model
            Wrapper.CommitToModel();
            
            // The model to save is the one inside the wrapper
            var staffToSave = Wrapper.Model;

            // 4. Permissions Sync
            if (Wrapper.LinkedUserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(Wrapper.LinkedUserId.Value);
                if (user != null)
                {
                    user.Permissions = CurrentPermissions;
                    await _userRepository.UpdateAsync(user);
                }
            }

            // 5. Persist to Repository
            try 
            {
                BusyText = "Saving employee details...";
                IsBusy = true;
                
                if (_existingStaffId.HasValue)
                {
                    await _staffRepository.UpdateAsync(staffToSave);
                }
                else
                {
                    await _staffRepository.AddAsync(staffToSave);
                }
                
                EmployeeAdded?.Invoke(this, EventArgs.Empty);
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Failed to save employee: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave() => Wrapper != null && !Wrapper.HasErrors;

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void SetSelectedBank(BankName bank)
        {
            SelectedBank = bank;
        }

        #endregion

        #region Methods

        public void Load(Employee staff)
        {
            if (staff == null) return;

            try 
            {
                IsBusy = true;
                _existingStaffId = staff.Id;
                Title = "Edit Employee";
                SaveButtonText = "Save Changes";

                Wrapper = new EmployeeWrapper(staff);
                Wrapper.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(EmployeeWrapper.FirstName) || e.PropertyName == nameof(EmployeeWrapper.LastName))
                        OnPropertyChanged(nameof(DisplayName));
                    
                    SaveCommand.NotifyCanExecuteChanged();
                };

                if (Wrapper.LinkedUserId.HasValue)
                {
                    Task.Run(async () => 
                    {
                        var user = await _userRepository.GetByIdAsync(Wrapper.LinkedUserId.Value);
                        if (user != null)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.Post(() => CurrentPermissions = user.Permissions);
                        }
                    });
                }
                
                UpdatePermissionsButtonVisibility();

                // Sanitize Leave Dates
                if (Wrapper.LeaveCycleStartDate.HasValue && Wrapper.LeaveCycleStartDate.Value < new DateTime(1900, 1, 1))
                {
                     Wrapper.LeaveCycleStartDate = null;
                }

                _ = RefreshBalanceAsync();
                
                UpdateAccrualRule();
                
                // Banking Logic
                LoadBankInfo(staff.BankName);

                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
                OnPropertyChanged(nameof(IsHourly));
                OnPropertyChanged(nameof(IsSalary));
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
                OnPropertyChanged(nameof(IsOtherBankSelected));
                
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                throw;
            }
        }

        private void LoadBankInfo(string? dbBankName)
        {
            var matched = false;
            if (!string.IsNullOrEmpty(dbBankName))
            {
                foreach (var bank in AvailableBanks)
                {
                    if (bank == BankName.None || bank == BankName.Other) continue;
                    if (GetEnumDescription(bank).Equals(dbBankName, StringComparison.OrdinalIgnoreCase))
                    {
                        SelectedBank = bank;
                        matched = true;
                        break;
                    }
                }
            }

            if (!matched && !string.IsNullOrEmpty(dbBankName))
            {
                SelectedBank = BankName.Other;
                CustomBankName = dbBankName;
            }
            else if (!matched)
            {
                SelectedBank = BankName.None;
                CustomBankName = string.Empty;
            }
        }

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return attribute?.Description ?? value.ToString();
        }

        partial void OnWrapperChanged(EmployeeWrapper value)
        {
            if (value != null)
            {
                value.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(EmployeeWrapper.IdNumber))
                    {
                         if (Wrapper.IdType == IdType.RSAId && Wrapper.IdNumber.Length >= 6)
                            CalculateDoBFromRsaId(Wrapper.IdNumber);
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.IdType))
                    {
                        if (Wrapper.IdType == IdType.RSAId && Wrapper.IdNumber.Length >= 6)
                            CalculateDoBFromRsaId(Wrapper.IdNumber);
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.Branch))
                    {
                        UpdateShiftTimes();
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.Role))
                    {
                         UpdatePermissionsButtonVisibility();
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.EmploymentType))
                    {
                        UpdateAccrualRule();
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.LeaveCycleStartDate))
                    {
                        UpdateSickLeaveCycleEnd();
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.FirstName) || e.PropertyName == nameof(EmployeeWrapper.LastName))
                    {
                         OnPropertyChanged(nameof(DisplayName));
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.LinkedUserId))
                    {
                         OnPropertyChanged(nameof(HasLinkedUser));
                         HandleLinkedUserChange(Wrapper.LinkedUserId);
                         UpdatePermissionsButtonVisibility();
                    }
                    else if (e.PropertyName == nameof(EmployeeWrapper.AnnualLeaveBalance) || e.PropertyName == nameof(EmployeeWrapper.EmploymentDate))
                    {
                         _ = RefreshBalanceAsync();
                    }
                    
                    SaveCommand.NotifyCanExecuteChanged();
                };
            }
        }

        private void HandleLinkedUserChange(Guid? userId)
        {
            if (userId.HasValue)
            {
                var selectedUser = AvailableUsers.FirstOrDefault(u => u.Id == userId.Value);
                if (selectedUser != null)
                {
                    // Update wrapper with user info if it's currently empty or we want to sync
                    if (string.IsNullOrWhiteSpace(Wrapper.FirstName)) Wrapper.FirstName = selectedUser.FirstName;
                    if (string.IsNullOrWhiteSpace(Wrapper.LastName)) Wrapper.LastName = selectedUser.LastName;
                    if (string.IsNullOrWhiteSpace(Wrapper.Email)) Wrapper.Email = selectedUser.Email;
                    if (string.IsNullOrWhiteSpace(Wrapper.Phone)) Wrapper.Phone = selectedUser.Phone ?? string.Empty;
                    
                    CurrentPermissions = selectedUser.Permissions;
                }
            }
        }

        private void UpdateAccrualRule()
        {
            if (Wrapper.EmploymentType == EmploymentType.Contract)
            {
                LeaveAccrualRule = "Accural: 1 day / 17 days (Annual) | 1 day / 26 days (Sick)";
                if (!_existingStaffId.HasValue && Wrapper.AnnualLeaveBalance == 0) 
                {
                    Wrapper.AnnualLeaveBalance = 0;
                    Wrapper.SickLeaveBalance = 0;
                }
            }
            else
            {
                LeaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";
                if (!_existingStaffId.HasValue && Wrapper.AnnualLeaveBalance == 0)
                {
                    Wrapper.SickLeaveBalance = 30;
                }
            }
            _ = RefreshBalanceAsync();
        }

        private void UpdateSickLeaveCycleEnd()
        {
            if (Wrapper.LeaveCycleStartDate.HasValue && Wrapper.LeaveCycleStartDate.Value > new DateTime(1900, 1, 1))
            {
                var endDate = Wrapper.LeaveCycleStartDate.Value.AddMonths(36).AddDays(-1);
                SickLeaveCycleEndDisplay = endDate.ToString("dd MMM yyyy");
            }
            else
            {
                SickLeaveCycleEndDisplay = "N/A";
            }
            _ = RefreshBalanceAsync();
        }

        private void UpdateShiftTimes()
        {
            var jhbStart = new TimeSpan(7, 0, 0);
            var jhbEnd = new TimeSpan(16, 45, 0);
            var cptStart = new TimeSpan(7, 0, 0);
            var cptEnd = new TimeSpan(16, 30, 0);

            if (string.Equals(Wrapper.Branch, "Johannesburg", StringComparison.OrdinalIgnoreCase))
            {
                Wrapper.ShiftStartTime = jhbStart;
                Wrapper.ShiftEndTime = jhbEnd;
            }
            else if (string.Equals(Wrapper.Branch, "Cape Town", StringComparison.OrdinalIgnoreCase))
            {
                Wrapper.ShiftStartTime = cptStart;
                Wrapper.ShiftEndTime = cptEnd;
            }
        }

        [RelayCommand]
        private void ClosePermissions()
        {
            IsPermissionsPopupVisible = false;
        }

        [RelayCommand]
        private async Task OpenPermissions()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser?.UserRole != UserRole.Admin)
            {
                await _dialogService.ShowAlertAsync("Access Denied", "Only administrators can manage user access and permissions.");
                return;
            }

            var linkedUser = AvailableUsers.FirstOrDefault(u => u.Id == Wrapper.LinkedUserId);
            var role = linkedUser?.UserRole ?? UserRole.Guest;

            var vm = new EmployeePermissionsViewModel(DisplayName, CurrentPermissions, role);
            vm.OnSaved = (p) => 
            {
                CurrentPermissions = p;
                IsPermissionsPopupVisible = false;
            };

            PermissionsPopupVM = vm;
            IsPermissionsPopupVisible = true;
        }

        private void UpdatePermissionsButtonVisibility()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser?.UserRole != UserRole.Admin)
            {
                ShowPermissionsButton = false;
                return;
            }

            // Only show for the Office employee role. 
            // SiteManagers are granted full access via direct role checks and don't need toggles.
            bool isRoleManaged = Wrapper.Role == EmployeeRole.Office;
            
            // Check if the linked user (if any) is an Admin. If so, hide permissions button.
            bool isLinkedAdmin = false;
            if (Wrapper.LinkedUserId.HasValue)
            {
                var linkedUser = AvailableUsers.FirstOrDefault(u => u.Id == Wrapper.LinkedUserId.Value);
                // Treat SiteManager as Admin for button visibility too, as they have full access for now
                isLinkedAdmin = linkedUser?.UserRole == UserRole.Admin || linkedUser?.UserRole == UserRole.SiteManager;
            }

            ShowPermissionsButton = isRoleManaged && !isLinkedAdmin;
        }

        private void CalculateDoBFromRsaId(string id)
        {
            if (id.Length < 6) return;
            string datePart = id.Substring(0, 6);
            
            if (DateTime.TryParseExact(datePart, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
            {
                _calculatedDoB = dob;
                Wrapper.DoB = dob;
            }
        }

        private async Task RefreshBalanceAsync()
        {
            if (_leaveService == null || IsBusy) return;

            try
            {
                // Create a temporary employee object to calculate balance without saving to DB
                var tempEmployee = new Employee
                {
                    Id = _existingStaffId ?? Guid.Empty,
                    AnnualLeaveBalance = Wrapper.AnnualLeaveBalance,
                    EmploymentDate = Wrapper.EmploymentDate
                };

                var balance = await _leaveService.CalculateCurrentLeaveBalanceAsync(tempEmployee);
                Avalonia.Threading.Dispatcher.UIThread.Post(() => CurrentAnnualLeaveBalance = balance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmployeeDetailViewModel] Error refreshing balance: {ex.Message}");
            }
        }

        #endregion
    }
}
