using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OCC.Shared.DTOs;
using OCC.Shared.Models;
using OCC.WpfClient.Infrastructure;
using OCC.WpfClient.Services.Interfaces;

namespace OCC.WpfClient.Features.Employees.ViewModels
{
    public partial class EmployeeListViewModel : ViewModelBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;
        private readonly ILogger<EmployeeListViewModel> _logger;
        private List<EmployeeSummaryDto> _allEmployees = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private int _selectedFilterIndex = 0; // 0 = Everyone, 1 = Permanent, 2 = Contract

        [ObservableProperty]
        private int _selectedBranchFilterIndex = 0; // 0 = All, 1 = JHB, 2 = CPT

        [ObservableProperty]
        private ObservableCollection<EmployeeSummaryDto> _employees = new();

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private int _permanentCount;

        [ObservableProperty]
        private int _contractCount;

        [ObservableProperty]
        private EmployeeSummaryDto? _selectedEmployee;


        [ObservableProperty]
        private EmployeeDetailViewModel? _detailViewModel = null;

        // Column Visibility
        [ObservableProperty] private bool _isNumberVisible = true;
        [ObservableProperty] private bool _isPositionVisible = true;
        [ObservableProperty] private bool _isTypeVisible = true;
        [ObservableProperty] private bool _isBranchVisible = true;

        [ObservableProperty]
        private bool _isColumnPickerOpen;

        public EmployeeListViewModel(IEmployeeService employeeService, IUserService userService, ILogger<EmployeeListViewModel> logger)
        {
            _employeeService = employeeService;
            _userService = userService;
            _logger = logger;
            Title = "Employees";
            DetailViewModel = null;
            
            _ = LoadData();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                IsBusy = true;
                BusyText = "Loading employees...";
                
                var employees = await _employeeService.GetEmployeesAsync();
                _allEmployees = employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();
                
                FilterEmployees();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var employee = new Models.EmployeeModel();
            DetailViewModel = new EmployeeDetailViewModel(this, employee, _employeeService, _userService, (ILogger)_logger);
        }

        [RelayCommand]
        private async Task EditEmployee(EmployeeSummaryDto? summary)
        {
            if (summary == null) return;
            
            try
            {
                IsBusy = true;
                BusyText = "Loading employee details...";
                var dto = await _employeeService.GetEmployeeAsync(summary.Id);
                if (dto != null)
                {
                    var model = new Models.EmployeeModel(dto);
                    DetailViewModel = new EmployeeDetailViewModel(this, model, _employeeService, _userService, (ILogger)_logger);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteEmployee(EmployeeSummaryDto? employee)
        {
            if (employee == null) return;
            
            // In a real app, we'd show a confirmation dialog here.
            // For now, let's assume confirmation and implement the logic.
            try
            {
                IsBusy = true;
                BusyText = "Deleting employee...";
                var success = await _employeeService.DeleteEmployeeAsync(employee.Id);
                if (success)
                {
                    await LoadData();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleColumnPicker() => IsColumnPickerOpen = !IsColumnPickerOpen;

        public void CloseDetailView()
        {
            DetailViewModel = null;
        }

        partial void OnSearchQueryChanged(string value) => FilterEmployees();
        partial void OnSelectedFilterIndexChanged(int value) => FilterEmployees();
        partial void OnSelectedBranchFilterIndexChanged(int value) => FilterEmployees();

        private void FilterEmployees()
        {
            var filtered = _allEmployees.AsEnumerable();

            // Search Query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(e => 
                    (e.FirstName?.ToLower().Contains(query) ?? false) ||
                    (e.LastName?.ToLower().Contains(query) ?? false) ||
                    (e.EmployeeNumber?.ToLower().Contains(query) ?? false));
            }

            // Employment Type Filter
            filtered = SelectedFilterIndex switch
            {
                1 => filtered.Where(e => e.EmploymentType == EmploymentType.Permanent),
                2 => filtered.Where(e => e.EmploymentType == EmploymentType.Contract),
                _ => filtered
            };

            // Branch Filter
            filtered = SelectedBranchFilterIndex switch
            {
                1 => filtered.Where(e => e.Branch == "Johannesburg"),
                2 => filtered.Where(e => e.Branch == "Cape Town"),
                _ => filtered
            };

            var result = filtered.ToList();
            Employees = new ObservableCollection<EmployeeSummaryDto>(result);

            // Update Stats
            TotalCount = result.Count;
            PermanentCount = result.Count(e => e.EmploymentType == EmploymentType.Permanent);
            ContractCount = result.Count(e => e.EmploymentType == EmploymentType.Contract);
        }
    }
}
