using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Shared.DTOs;
using OCC.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class AddLoanDialogViewModel : ObservableObject
    {
        private readonly IEmployeeService _employeeService;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        
        public Action<EmployeeLoan?>? CloseAction { get; set; }

        public AddLoanDialogViewModel(
            IEmployeeService employeeService,
            ISettingsService settingsService,
            IDialogService dialogService)
        {
            _employeeService = employeeService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            
            StartDate = DateTime.Today;
            LoadData();
        }

        private async void LoadData()
        {
            await LoadEmployees();
            await LoadSettings();
        }

        private async Task LoadSettings()
        {
            try
            {
                var companySettings = await _settingsService.GetCompanyDetailsAsync();
                InterestRate = companySettings.GlobalLoanInterestRate;
            }
            catch (Exception)
            {
                // Fallback or log?
                InterestRate = 0; 
            }
        }

        [ObservableProperty]
        private ObservableCollection<EmployeeSummaryDto> _employees = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private EmployeeSummaryDto? _selectedEmployee;

        [ObservableProperty]
        private decimal _principalAmount;

        [ObservableProperty]
        private decimal _monthlyInstallment;

        [ObservableProperty]
        private DateTime _startDate;
        
        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private decimal _interestRate;

        private async Task LoadEmployees()
        {
            var employees = await _employeeService.GetEmployeesAsync();
            Employees = new ObservableCollection<EmployeeSummaryDto>(employees.OrderBy(e => e.FirstName));
            if (Employees.Any()) SelectedEmployee = Employees.First();
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private void Save()
        {
            if (SelectedEmployee == null) return;

            var loan = new EmployeeLoan
            {
                EmployeeId = SelectedEmployee.Id,
                PrincipalAmount = PrincipalAmount,
                OutstandingBalance = PrincipalAmount, // Initially same
                MonthlyInstallment = MonthlyInstallment,
                StartDate = StartDate,
                IsActive = true,
                InterestRate = InterestRate,
                Notes = Notes
                // Employee is not populated fully, just ID.
            };
            
            CloseAction?.Invoke(loan);
        }

        private bool CanSave() => SelectedEmployee != null && PrincipalAmount > 0;

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke(null);
        }

        [RelayCommand]
        private async Task ReportBug()
        {
            await _dialogService.ShowBugReportAsync("AddLoanDialog");
        }
    }
}
