using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Shared.Models;
using OCC.Client.ViewModels.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace OCC.Client.ViewModels.HealthSafety
{
    public partial class TrainingViewModel : ViewModelBase
    {
        private readonly IHealthSafetyService _hseqService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;
        private readonly IRepository<Employee> _employeeRepository;

        [ObservableProperty]
        private ObservableCollection<TrainingRecordViewModel> _trainingRecords = new();

        [ObservableProperty]
        private ObservableCollection<string> _certificateTypes = new();

        [ObservableProperty]
        private int _expiryWarningDays = 30;

        [ObservableProperty]
        private ObservableCollection<Employee> _employees = new();

        [ObservableProperty]
        private Employee? _selectedEmployee;

        [ObservableProperty]
        private string _certificateFileName = "No file selected";

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
            IRepository<Employee> employeeRepository)
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
                "Medicals",
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
                // Create a new instance to force UI update binding
                NewRecord = new HseqTrainingRecord
                {
                    Id = NewRecord.Id,
                    EmployeeName = value.DisplayName,
                    Role = value.Role.ToString(),
                    EmployeeId = value.Id,
                    // Preserve other fields
                    TrainingTopic = NewRecord.TrainingTopic,
                    DateCompleted = NewRecord.DateCompleted == default ? DateTime.Now : NewRecord.DateCompleted,
                    ValidUntil = NewRecord.ValidUntil ?? DateTime.Now,
                    Trainer = NewRecord.Trainer,
                    CertificateUrl = NewRecord.CertificateUrl,
                    CertificateType = NewRecord.CertificateType,
                    ExpiryWarningDays = NewRecord.ExpiryWarningDays
                };
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

                var vms = recordsTask.Result.Select(r => new TrainingRecordViewModel(r)).ToList();
                TrainingRecords = new ObservableCollection<TrainingRecordViewModel>(vms);
                
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
                var vms = expiring.Select(r => new TrainingRecordViewModel(r)).ToList();
                TrainingRecords = new ObservableCollection<TrainingRecordViewModel>(vms);
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
        private async Task DeleteRecord(TrainingRecordViewModel vm)
        {
            if (vm == null) return;
            
            var confirm = await _dialogService.ShowConfirmationAsync("Confirm Delete", $"Delete training record for {vm.EmployeeName}?");
            if (confirm)
            {
                var success = await _hseqService.DeleteTrainingRecordAsync(vm.Id);
                if (success)
                {
                    TrainingRecords.Remove(vm);
                    _toastService.ShowSuccess("Success", "Record deleted.");
                }
            }
        }

        [RelayCommand]
        private async Task UploadCertificate()
        {
            try
            {
                var path = await _dialogService.PickFileAsync("Select Certificate", new[] { "pdf", "jpg", "jpeg", "png" });
                if (!string.IsNullOrEmpty(path))
                {
                    NewRecord.CertificateUrl = path;
                    CertificateFileName = Path.GetFileName(path);
                    _toastService.ShowSuccess("File Selected", CertificateFileName);
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Error", "Failed to select file.");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        [ObservableProperty]
        private HseqTrainingRecord _newRecord = new() 
        { 
            DateCompleted = DateTime.Now, 
            ValidUntil = DateTime.Now 
        };

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
                // Upload Certificate if local file exists
                if (!string.IsNullOrEmpty(NewRecord.CertificateUrl) && File.Exists(NewRecord.CertificateUrl))
                {
                    try
                    {
                        using var stream = File.OpenRead(NewRecord.CertificateUrl);
                        var fileName = Path.GetFileName(NewRecord.CertificateUrl);
                        var serverUrl = await _hseqService.UploadCertificateAsync(stream, fileName);
                        
                        if (!string.IsNullOrEmpty(serverUrl))
                        {
                            NewRecord.CertificateUrl = serverUrl;
                        }
                    }
                    catch (Exception ex)
                    {
                        _toastService.ShowError("Upload Failed", "Could not upload certificate. Saving text only.");
                        System.Diagnostics.Debug.WriteLine($"Upload error: {ex.Message}");
                    }
                }

                NewRecord.TrainingTopic = NewRecord.CertificateType; 

                var created = await _hseqService.CreateTrainingRecordAsync(NewRecord);
                if (created != null)
                {
                    TrainingRecords.Insert(0, new TrainingRecordViewModel(created));
                    _toastService.ShowSuccess("Saved", "Training record added.");
                    NewRecord = new HseqTrainingRecord 
                    { 
                        DateCompleted = DateTime.Now, 
                        ValidUntil = DateTime.Now 
                    }; 
                    CertificateFileName = "No file selected";
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

    /// <summary>
    /// Wrapper for HseqTrainingRecord to provide UI-specific properties like Validity.
    /// </summary>
    public class TrainingRecordViewModel : ObservableObject
    {
        public HseqTrainingRecord Record { get; }

        public TrainingRecordViewModel(HseqTrainingRecord record)
        {
            Record = record;
        }

        // Expose properties for binding
        public Guid Id => Record.Id;
        public string EmployeeName => Record.EmployeeName;
        public string Role => Record.Role;
        public string TrainingTopic => Record.TrainingTopic;
        public DateTime DateCompleted => Record.DateCompleted;
        public DateTime? ValidUntil => Record.ValidUntil;
        public string Trainer => Record.Trainer;

        public string ValidityStatus
        {
            get
            {
                if (!ValidUntil.HasValue) return "No Expiry";
                
                var daysLeft = (ValidUntil.Value.Date - DateTime.Now.Date).Days;
                
                if (daysLeft < 0) return $"Expired ({Math.Abs(daysLeft)} days ago)";
                if (daysLeft == 0) return "Expires Today";
                if (daysLeft < 30) return $"Expires in {daysLeft} days"; // Warning range
                
                // For longer durations, maybe years/months?
                if (daysLeft > 365) 
                {
                    var years = Math.Round(daysLeft / 365.25, 1);
                    return $"{years} Years";
                }
                
                return $"{daysLeft} Days";
            }
        }
    }
}
