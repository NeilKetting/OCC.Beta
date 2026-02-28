using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System.Linq;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class WageRunLineViewModel : ObservableObject
    {
        [ObservableProperty]
        private WageRunLine _model;

        public WageRunLineViewModel(WageRunLine model)
        {
            _model = model;
        }

        // Expose properties for binding
        public string EmployeeName => Model.EmployeeName;
        public string Branch => Model.Branch;
        
        public double NormalHours => Model.NormalHours;
        public double Overtime15Hours => Model.Overtime15Hours;
        public double Overtime20Hours => Model.Overtime20Hours;
        public double ProjectedHours => Model.ProjectedHours;
        public double VarianceHours => Model.VarianceHours;
        public double LunchDeductionHours => Model.LunchDeductionHours;
        
        public string VarianceNotes => Model.VarianceNotes;

        // Calculated Total Hours (Paid Normal Hours essentially)
        // Note: Overtime is paid separately generally, but if we just want a "Hours Count"
        public double TotalPaidHours => NormalHours + ProjectedHours + VarianceHours; 

        public decimal HourlyRate => Model.HourlyRate;
        public decimal RatePerDay => Model.RatePerDay;
        public double HoursPerDay => Model.HoursPerDay;
        
        // Left Side Rates
        public decimal RatePerHour => Model.RatePerHour;
        public decimal StdOvertimeRate => Model.StdOvertimeRate;
        public decimal SatOvertimeRate => Model.SatOvertimeRate;
        public decimal SunOvertimeRate => Model.SunOvertimeRate;
        public decimal DecRate => Model.DecRate;
        
        // Editable Fields
        public double DaysWeek1
        {
            get => Model.DaysWeek1;
            set { if (Model.DaysWeek1 != value) { Model.DaysWeek1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalDays)); RecalculateWage(); } }
        }
        
        public double DaysWeek2
        {
            get => Model.DaysWeek2;
            set { if (Model.DaysWeek2 != value) { Model.DaysWeek2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalDays)); RecalculateWage(); } }
        }

        public double TotalDays => DaysWeek1 + DaysWeek2;

        public double StdOvertime
        {
            get => Model.StdOvertime;
            set { if (Model.StdOvertime != value) { Model.StdOvertime = value; OnPropertyChanged(); RecalculateWage(); } }
        }

        public double SatOvertime
        {
            get => Model.SatOvertime;
            set { if (Model.SatOvertime != value) { Model.SatOvertime = value; OnPropertyChanged(); RecalculateWage(); } }
        }

        public double SunOvertime
        {
            get => Model.SunOvertime;
            set { if (Model.SunOvertime != value) { Model.SunOvertime = value; OnPropertyChanged(); RecalculateWage(); } }
        }

        public decimal DeductionWashing
        {
            get => Model.DeductionWashing;
            set { if (Model.DeductionWashing != value) { Model.DeductionWashing = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetPay)); } }
        }

        public decimal DeductionGas
        {
            get => Model.DeductionGas;
            set { if (Model.DeductionGas != value) { Model.DeductionGas = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetPay)); } }
        }
        
        public double DecHrs
        {
            get => Model.DecHrs;
            set { if (Model.DecHrs != value) { Model.DecHrs = value; OnPropertyChanged(); } }
        }

        public decimal DecTotal
        {
            get => Model.DecTotal;
            set { if (Model.DecTotal != value) { Model.DecTotal = value; OnPropertyChanged(); } }
        }

        public string? BankName => Model.BankName;
        public string? AccountNumber => Model.AccountNumber;

        public decimal TotalWage => Model.TotalWage;

        public decimal DeductionLoan => Model.DeductionLoan;
        public decimal DeductionTax => Model.DeductionTax;
        public decimal DeductionOther => Model.DeductionOther;

        public decimal IncentiveSupervisor
        {
            get => Model.IncentiveSupervisor;
            set
            {
                if (Model.IncentiveSupervisor != value)
                {
                    Model.IncentiveSupervisor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NetPay));
                }
            }
        }

        public decimal NetPay => Model.NetPay;

        private void RecalculateWage()
        {
            Model.TotalWage = (decimal)TotalDays * Model.RatePerDay +
                              (decimal)Model.VarianceHours * Model.HourlyRate +
                              (decimal)Model.StdOvertime * Model.HourlyRate * 1.5m +
                              (decimal)Model.SatOvertime * Model.HourlyRate * 1.5m +
                              (decimal)Model.SunOvertime * Model.HourlyRate * 2.0m;
            
            OnPropertyChanged(nameof(TotalWage));
            OnPropertyChanged(nameof(NetPay));
        }

        public bool HasVariance => System.Math.Abs(VarianceHours) > 0.01;

        public string VarianceColor
        {
            get
            {
                if (VarianceHours < 0) return "#EF4444"; // Red (Deduction)
                if (VarianceHours > 0) return "#10B981"; // Green (Addition)
                return "Transparent";
            }
        }
    }
}
