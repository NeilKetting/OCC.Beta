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

        public EmployeeListViewModel(IEmployeeService employeeService, ILogger<EmployeeListViewModel> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
            Title = "Employees";
            
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
            // TODO: Implement Add Employee Dialog
        }

        [RelayCommand]
        private void EditEmployee(EmployeeSummaryDto? employee)
        {
            if (employee == null) return;
            // TODO: Implement Edit Employee Dialog
        }

        [RelayCommand]
        private void DeleteEmployee(EmployeeSummaryDto? employee)
        {
            if (employee == null) return;
            // TODO: Implement Delete Confirmation and Logic
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
