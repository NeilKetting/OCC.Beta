using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Repositories.Interfaces; // Added
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class TrainingViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;
        private readonly IRepository<Employee> _employeeRepository; // Added

        [ObservableProperty]
        private ObservableCollection<HseqTrainingRecord> _trainingRecords = new();

        [ObservableProperty]
        private ObservableCollection<string> _certificateTypes = new();

        [ObservableProperty]
        private int _expiryWarningDays = 30;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new(); // Added

        [ObservableProperty]
        private Employee? _selectedEmployee; // Added

        public TrainingViewModel()
        {
            // Design-time
            _hseqService = null!;
            _dialogService = null!;
            _toastService = null!;
            _employeeRepository = null!;
            InitializeCertificateTypes();
        }

        public TrainingViewModel(
            IHealthSafetyService hseqService, 
            IDialogService dialogService, 
            IToastService toastService,
            IRepository<Employee> employeeRepository) // Modified
        {
            _hseqService = hseqService;
            _dialogService = dialogService;
            _toastService = toastService;
            _employeeRepository = employeeRepository;
            InitializeCertificateTypes();
            LoadDataCommand.ExecuteAsync(null);
        }

        private void InitializeCertificateTypes()
        {
            CertificateTypes = new ObservableCollection<string>
            {
                "First Aid Level 1",
                "First Aid Level 2",
                "First Aid Level 3",
                "SHE Representative",
                "Basic Fire Fighting",
                "Advanced Fire Fighting",
                "HIRA (Hazard Identification & Risk Assessment)",
                "Scaffolding Erector",
                "Scaffolding Inspector",
                "Working at Heights",
                "Fall Protection Planner",
                "Confined Space Entry",
                "Incident Investigation",
                "Legal Liability",
                "Construction Regulations",
                "Excavation Supervisor",
                "Demolition Supervisor"
            };
        }

        partial void OnSelectedEmployeeChanged(Employee? value)
        {
            if (value != null)
            {
                NewRecord.EmployeeName = value.DisplayName;
                NewRecord.Role = value.Role.ToString(); // Auto-fill Role
            }
        }

        [RelayCommand]
        private async Task LoadData()
        {
            if (_hseqService == null) return;

            IsBusy = true;
            try
            {
                // Parallel load
                var recordsTask = _hseqService.GetTrainingRecordsAsync();
                var employeesTask = _employeeRepository.GetAllAsync();

                await Task.WhenAll(recordsTask, employeesTask);

                TrainingRecords = new ObservableCollection<HseqTrainingRecord>(recordsTask.Result);
                Employees = new ObservableCollection<Employee>(employeesTask.Result.OrderBy(e => e.FirstName).ThenBy(e => e.LastName));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading training data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FilterExpiring()
        {
            IsBusy = true;
            try
            {
                var expiring = await _hseqService.GetExpiringTrainingAsync(ExpiryWarningDays);
                TrainingRecords = new ObservableCollection<HseqTrainingRecord>(expiring);
                _toastService.ShowInfo("Filter Applied", $"Found {expiring.Count()} records expiring within {ExpiryWarningDays} days.");
            }
            catch (Exception)
            {
                 _toastService.ShowError("Error", "Failed to filter records.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeleteRecord(HseqTrainingRecord record)
        {
            if (record == null) return;
            
            var confirm = await _dialogService.ShowConfirmationAsync("Confirm Delete", $"Delete training record for {record.EmployeeName}?");
            if (confirm)
            {
                var success = await _hseqService.DeleteTrainingRecordAsync(record.Id);
                if (success)
                {
                    TrainingRecords.Remove(record);
                    _toastService.ShowSuccess("Success", "Record deleted.");
                }
            }
        }

        [ObservableProperty]
        private HseqTrainingRecord _newRecord = new();

        [RelayCommand]
        private async Task SaveTraining()
        {
            if (string.IsNullOrWhiteSpace(NewRecord.EmployeeName) || string.IsNullOrWhiteSpace(NewRecord.CertificateType))
            {
                _toastService.ShowError("Validation", "Employee Name and Certificate Type are required.");
                return;
            }

            IsBusy = true;
            try
            {
                // Default valid until based on type? Usually 1-3 years. Let's assume 2 years for now if not set.
                if (!NewRecord.ValidUntil.HasValue)
                {
                    NewRecord.ValidUntil = NewRecord.DateCompleted.AddYears(2);
                }
                
                // Map Combobox selection to Topic if needed, or just use CertificateType property
                NewRecord.TrainingTopic = NewRecord.CertificateType; 

                var created = await _hseqService.CreateTrainingRecordAsync(NewRecord);
                if (created != null)
                {
                    TrainingRecords.Insert(0, created);
                    _toastService.ShowSuccess("Saved", "Training record added.");
                    NewRecord = new HseqTrainingRecord(); // Reset
                    SelectedEmployee = null; // Reset selection
                }
            }
            catch(Exception)
            {
                _toastService.ShowError("Error", "Failed to save record.");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
