using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace OCC.Client.ViewModels.EmployeeManagement
{
    public partial class EmployeeDetailViewModel : ViewModelBase
    {
        private readonly IRepository<Employee> _staffRepository;

        public event EventHandler? CloseRequested;
        public event EventHandler? EmployeeAdded;

        // Form Properties matching View
        [ObservableProperty]
        private string _employeeNumber = string.Empty;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _idNumber = string.Empty;

        [ObservableProperty]
        private IdType _selectedIdType = IdType.RSAId;

        [ObservableProperty]
        private string _phone = string.Empty; // Not in Model yet, but in View

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private StaffRole _selectedSkill = StaffRole.GeneralWorker;

        [ObservableProperty]
        private double _hourlyRate;

        // Employment Date (Not in model explicitly, using as placeholder or maybe DoB? No, DoB is calculated)
        // We will store it but it won't persist to StaffMember unless we add a field later.
        [ObservableProperty]
        private DateTimeOffset _employmentDate = DateTimeOffset.Now;

        [ObservableProperty]
        private EmploymentType _selectedEmploymentType = EmploymentType.Permanent;

        [ObservableProperty]
        private string _contractDuration = string.Empty; // For UI binding

        // Helper boolean properties for RadioButtons
        public bool IsRsaId
        {
            get => SelectedIdType == IdType.RSAId;
            set
            {
                if (value) SelectedIdType = IdType.RSAId;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPassport
        {
            get => SelectedIdType == IdType.Passport;
            set
            {
                if (value) SelectedIdType = IdType.Passport;
                OnPropertyChanged(nameof(IsRsaId));
                OnPropertyChanged(nameof(IsPassport));
            }
        }

        public bool IsPermanent
        {
            get => SelectedEmploymentType == EmploymentType.Permanent;
            set
            {
                if (value) SelectedEmploymentType = EmploymentType.Permanent;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContract
        {
            get => SelectedEmploymentType == EmploymentType.Contract;
            set
            {
                if (value) SelectedEmploymentType = EmploymentType.Contract;
                OnPropertyChanged(nameof(IsPermanent));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsContractVisible));
            }
        }

        public bool IsContractVisible => IsContract;

        // Calculated DoB
        private DateTime _calculatedDoB = DateTime.Now.AddYears(-30);

        public EmployeeDetailViewModel(IRepository<Employee> staffRepository)
        {
            _staffRepository = staffRepository;
        }

        // Parameterless constructor for design-time or empty init
        public EmployeeDetailViewModel() 
        {
            // _staffRepository will be null, handle in Save
        }

        partial void OnIdNumberChanged(string value)
        {
            if (SelectedIdType == IdType.RSAId && value.Length >= 6)
            {
                CalculateDoBFromRsaId(value);
            }
        }

        partial void OnSelectedIdTypeChanged(IdType value)
        {
            // Re-validate/calc if needed
             if (value == IdType.RSAId && IdNumber.Length >= 6)
            {
                CalculateDoBFromRsaId(IdNumber);
            }
        }

        private void CalculateDoBFromRsaId(string id)
        {
            // YYMMDD
            if (id.Length < 6) return;
            string datePart = id.Substring(0, 6);
            
            // Try parse using yyMMdd
            if (DateTime.TryParseExact(datePart, "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
            {
                _calculatedDoB = dob;
            }
        }

        private Guid? _existingStaffId;

        [ObservableProperty]
        private string _title = "Add Employee";

        [ObservableProperty]
        private string _saveButtonText = "Add Employee";

        public void Load(Employee staff)
        {
            if (staff == null) return;

            _existingStaffId = staff.Id;
            Title = "Edit Employee";
            SaveButtonText = "Save Changes";

            EmployeeNumber = staff.EmployeeNumber;
            FirstName = staff.FirstName;
            LastName = staff.LastName;
            IdNumber = staff.IdNumber;
            SelectedIdType = staff.IdType;
            Email = staff.Email;
            SelectedSkill = staff.Role;
            HourlyRate = staff.HourlyRate;
            SelectedEmploymentType = staff.EmploymentType;
            // EmploymentDate and Phone are not on model yet
            
            // Trigger property change notifications for radio buttons
            OnPropertyChanged(nameof(IsRsaId));
            OnPropertyChanged(nameof(IsPassport));
            OnPropertyChanged(nameof(IsPermanent));
            OnPropertyChanged(nameof(IsContract));
            OnPropertyChanged(nameof(IsContractVisible));
        }

        [RelayCommand]
        private async Task Save()
        {
            // Basic Validation
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
                return;

            Employee staff;

            if (_existingStaffId.HasValue)
            {
                // Update existing
                staff = await _staffRepository.GetByIdAsync(_existingStaffId.Value) ?? new Employee { Id = _existingStaffId.Value };
            }
            else
            {
                // Create new
                staff = new Employee();
            }

            // Map properties
            staff.EmployeeNumber = EmployeeNumber;
            staff.FirstName = FirstName;
            staff.LastName = LastName;
            staff.IdNumber = IdNumber;
            staff.IdType = SelectedIdType;
            staff.Email = Email;
            staff.Role = SelectedSkill;
            staff.HourlyRate = HourlyRate;
            staff.EmploymentType = SelectedEmploymentType;
            staff.DoB = _calculatedDoB;

            if (_staffRepository != null)
            {
                if (_existingStaffId.HasValue)
                {
                    await _staffRepository.UpdateAsync(staff);
                }
                else
                {
                    await _staffRepository.AddAsync(staff);
                }
            }
            
            EmployeeAdded?.Invoke(this, EventArgs.Empty);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
