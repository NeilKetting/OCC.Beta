using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System.Linq;

namespace OCC.Client.Features.EmployeeHub.ViewModels
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
