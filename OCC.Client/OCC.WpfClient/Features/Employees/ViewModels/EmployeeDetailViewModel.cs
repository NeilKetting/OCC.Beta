using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OCC.WpfClient.Features.Employees.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;
using OCC.Shared.Models;
using System.Collections.Generic;

namespace OCC.WpfClient.Features.Employees.ViewModels
{
    public partial class EmployeeDetailViewModel : ViewModelBase
    {
        private readonly EmployeeListViewModel _parent;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger _logger;

        [ObservableProperty]
        private EmployeeModel _employee;

        [ObservableProperty]
        private bool _isNew;

        public List<EmployeeRole> Roles { get; } = new(Enum.GetValues<EmployeeRole>());
        public List<EmploymentType> EmploymentTypes { get; } = new(Enum.GetValues<EmploymentType>());
        public List<IdType> IdTypes { get; } = new(Enum.GetValues<IdType>());
        public List<string> Branches { get; } = new() { "Johannesburg", "Cape Town", "Durban" };
        public List<string> AccountTypes { get; } = new() { "Savings", "Cheque", "Transmission" };
        public List<BankName> Banks { get; } = new(Enum.GetValues<BankName>());

        [ObservableProperty]
        private List<User> _availableUsers = new();

        [ObservableProperty]
        private string _leaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";

        [ObservableProperty]
        private double _currentAnnualLeaveBalance;

        [ObservableProperty]
        private string _sickLeaveCycleEndDisplay = "N/A";

        [ObservableProperty]
        private bool _isSystemAccessVisible;

        [ObservableProperty]
        private bool _showPermissionsButton;

        public bool IsPassportVisible => Employee.IdType == IdType.Passport;
        public bool IsContractVisible => Employee.EmploymentType == EmploymentType.Contract;

        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public EmployeeDetailViewModel(EmployeeListViewModel parent, EmployeeModel employee, IEmployeeService employeeService, IUserService userService, IAuthService authService, ILogger logger)
        {
            _parent = parent;
            _employee = employee;
            _employeeService = employeeService;
            _userService = userService;
            _authService = authService;
            _logger = logger;
            _isNew = employee.Id == Guid.Empty;
            
            Title = _isNew ? "Add Employee" : $"Edit {employee.DisplayName}";

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var users = await _userService.GetUsersAsync();
                AvailableUsers = users.OrderBy(u => u.DisplayName).ToList();

                UpdateSystemAccessVisibility();
                UpdatePermissionsButtonVisibility();
                UpdateAccrualRule();
                UpdateSickLeaveCycleEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for employee link");
            }
        }

        partial void OnEmployeeChanged(EmployeeModel value)
        {
            if (value != null)
            {
                value.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(EmployeeModel.IdNumber))
                    {
                        if (Employee.IdType == IdType.RSAId)
                            CalculateDoBFromRsaId(Employee.IdNumber);
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.IdType))
                    {
                        OnPropertyChanged(nameof(IsPassportVisible));
                        if (Employee.IdType == IdType.RSAId)
                            CalculateDoBFromRsaId(Employee.IdNumber);
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.Branch))
                    {
                        UpdateShiftTimes();
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.Role))
                    {
                        UpdateSystemAccessVisibility();
                        UpdatePermissionsButtonVisibility();
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.EmploymentType))
                    {
                        OnPropertyChanged(nameof(IsContractVisible));
                        UpdateAccrualRule();
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.LeaveCycleStartDate))
                    {
                        UpdateSickLeaveCycleEnd();
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.LinkedUserId))
                    {
                        HandleLinkedUserChange(Employee.LinkedUserId);
                        UpdatePermissionsButtonVisibility();
                    }
                };
            }
        }

        private void CalculateDoBFromRsaId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length < 6) return;
            string datePart = id.Substring(0, 6);
            
            if (DateTime.TryParseExact(datePart, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime dob))
            {
                // Simple assumption for century (current window is 1920-2019)
                if (dob > DateTime.Now) dob = dob.AddYears(-100);
                Employee.DoB = dob;
            }
        }

        private void UpdateShiftTimes()
        {
            var jhbStart = new TimeSpan(7, 0, 0);
            var jhbEnd = new TimeSpan(16, 45, 0);
            var cptStart = new TimeSpan(7, 0, 0);
            var cptEnd = new TimeSpan(16, 30, 0);

            if (string.Equals(Employee.Branch, "Johannesburg", StringComparison.OrdinalIgnoreCase))
            {
                Employee.ShiftStartTime = jhbStart;
                Employee.ShiftEndTime = jhbEnd;
            }
            else if (string.Equals(Employee.Branch, "Cape Town", StringComparison.OrdinalIgnoreCase))
            {
                Employee.ShiftStartTime = cptStart;
                Employee.ShiftEndTime = cptEnd;
            }
        }

        private void UpdateAccrualRule()
        {
            if (Employee.EmploymentType == EmploymentType.Contract)
            {
                LeaveAccrualRule = "Accrual: 1 day / 17 days (Annual) | 1 day / 26 days (Sick)";
            }
            else
            {
                LeaveAccrualRule = "Standard: 15 Working Days Annual / 30 Days Sick Leave Cycle";
            }
        }

        private void UpdateSickLeaveCycleEnd()
        {
            if (Employee.LeaveCycleStartDate.HasValue && Employee.LeaveCycleStartDate.Value > new DateTime(1900, 1, 1))
            {
                var endDate = Employee.LeaveCycleStartDate.Value.AddMonths(36).AddDays(-1);
                SickLeaveCycleEndDisplay = endDate.ToString("dd MMM yyyy");
            }
            else
            {
                SickLeaveCycleEndDisplay = "N/A";
            }
        }

        private void UpdateSystemAccessVisibility()
        {
            IsSystemAccessVisible = Employee.Role == EmployeeRole.Office || Employee.Role == EmployeeRole.SiteManager;
        }

        private void UpdatePermissionsButtonVisibility()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser?.UserRole != UserRole.Admin)
            {
                ShowPermissionsButton = false;
                return;
            }

            bool isRoleManaged = Employee.Role == EmployeeRole.Office;
            bool isLinkedAdmin = false;

            if (Employee.LinkedUserId.HasValue)
            {
                var linkedUser = AvailableUsers.FirstOrDefault(u => u.Id == Employee.LinkedUserId.Value);
                isLinkedAdmin = linkedUser?.UserRole == UserRole.Admin || linkedUser?.UserRole == UserRole.SiteManager;
            }

            ShowPermissionsButton = isRoleManaged && !isLinkedAdmin;
        }

        private void HandleLinkedUserChange(Guid? userId)
        {
            if (userId.HasValue)
            {
                var selectedUser = AvailableUsers.FirstOrDefault(u => u.Id == userId.Value);
                if (selectedUser != null)
                {
                    if (string.IsNullOrWhiteSpace(Employee.FirstName)) Employee.FirstName = selectedUser.FirstName ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(Employee.LastName)) Employee.LastName = selectedUser.LastName ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(Employee.Email)) Employee.Email = selectedUser.Email ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(Employee.Phone)) Employee.Phone = selectedUser.Phone ?? string.Empty;
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            Employee.Validate();
            if (Employee.HasErrors) return;

            try
            {
                IsBusy = true;
                BusyText = "Saving employee...";

                bool success;
                if (IsNew)
                {
                    var result = await _employeeService.CreateEmployeeAsync(Employee.ToEntity());
                    success = result != null;
                }
                else
                {
                    success = await _employeeService.UpdateEmployeeAsync(Employee.ToEntity());
                }

                if (success)
                {
                    await _parent.LoadDataCommand.ExecuteAsync(null);
                    _parent.CloseDetailView();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving employee");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _parent.CloseDetailView();
        }

        [RelayCommand]
        private void Print()
        {
            _logger.LogInformation("Print Profile requested for {DisplayName}", Employee.DisplayName);
        }

        [RelayCommand]
        private void OpenPermissions()
        {
            _logger.LogInformation("Open Permissions requested for {DisplayName}", Employee.DisplayName);
        }
    }
}
