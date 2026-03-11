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

        private readonly IUserService _userService;

        public EmployeeDetailViewModel(EmployeeListViewModel parent, EmployeeModel employee, IEmployeeService employeeService, IUserService userService, ILogger logger)
        {
            _parent = parent;
            _employee = employee;
            _employeeService = employeeService;
            _userService = userService;
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
                        if (Employee.IdType == IdType.RSAId)
                            CalculateDoBFromRsaId(Employee.IdNumber);
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.Branch))
                    {
                        UpdateShiftTimes();
                    }
                    else if (e.PropertyName == nameof(EmployeeModel.LinkedUserId))
                    {
                        HandleLinkedUserChange(Employee.LinkedUserId);
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
            if (Employee.Branch == "Johannesburg")
            {
                Employee.ShiftStartTime = new TimeSpan(7, 0, 0);
                Employee.ShiftEndTime = new TimeSpan(16, 45, 0);
            }
            else if (Employee.Branch == "Cape Town")
            {
                Employee.ShiftStartTime = new TimeSpan(7, 0, 0);
                Employee.ShiftEndTime = new TimeSpan(16, 30, 0);
            }
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
    }
}
