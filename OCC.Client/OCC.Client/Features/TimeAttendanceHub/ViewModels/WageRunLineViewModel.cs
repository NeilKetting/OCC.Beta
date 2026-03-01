using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.Models;
using System;
using System.Linq;

namespace OCC.Client.Features.TimeAttendanceHub.ViewModels
{
    public partial class WageRunLineViewModel : ObservableObject
    {
        [ObservableProperty]
        private WageRunLine _model;

        private int? _index;
        public int? IndexNum
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public WageRunLineViewModel(WageRunLine model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        // --- 27 COLUMNS AS PER IMAGE ---
        
        // 1. #
        public string Index => IndexNum?.ToString() ?? string.Empty;

        // 2. BAS
        public string EmployeeNumber => Model?.EmployeeNumber ?? string.Empty;

        // 3. NAME
        public string EmployeeName => Model?.EmployeeName?.ToUpper() ?? string.Empty;

        // 4. RATE P/HR
        public decimal? RatePHrDisplay => Model?.HourlyRate;

        // 5. HRS
        public double? HrsDisplay => Model?.NormalHours;

        // 6. STD O/T RATE (1.5x)
        public decimal? StdOtRate => Model?.HourlyRate * 1.5m;

        // 7. SAT O/T RATE (1.5x)
        public decimal? SatOtRate => Model?.HourlyRate * 1.5m;

        // 8. SUN-P'HOL RATE (2.0x)
        public decimal? SunPHolRate => Model?.HourlyRate * 2.0m;

        // 9. DEC RATE - REMOVED PER USER

        // 10. DEC HRS - REMOVED PER USER

        // 11. DEC TOTAL
        public decimal? DecTotal => 0;

        // 12. $TD O/T (Hours)
        public double? StdOt => Model?.Overtime15Hours;

        // 13. SAT O/T (Hours)
        public double? SatOt => 0;

        // 14. SUN O/T (Hours)
        public double? SunOt => Model?.Overtime20Hours;

        // 15. LOANS
        public string DeductionLoan => (Model?.DeductionLoan > 0 ? Model.DeductionLoan.ToString("F2") : string.Empty);

        // 16. WASHING
        public string DeductionWashingDisplay => (Model?.DeductionWashing > 0 ? Model.DeductionWashing.ToString("F2") : string.Empty);

        // 17. GAS
        public string DeductionGasDisplay => (Model?.DeductionGas > 0 ? Model.DeductionGas.ToString("F2") : string.Empty);

        // 18. OTHER
        public string OtherDisplay => (Model?.DeductionOther > 0) ? Model.DeductionOther.ToString("F2") : string.Empty;

        // 19. TOTAL NETT / Bind to NetPay in user's XAML
        public decimal NetPay => BasicNett + IncentiveSupervisor;

        // --- COMPUTED SECTION (BLUE IN IMAGE) ---

        // 20. TOTAL REM
        public decimal TotalRem => Model?.TotalWage ?? 0;

        // 21. RATE P/DAY
        public decimal? RatePDayDisplay => Model?.HourlyRate * 8.75m;

        // 22. DAYS WEEK 1
        public double? DaysWeek1Display => (Model?.NormalHours > 0 ? Model.NormalHours / 8.75 : 0);

        // 23. DAYS WEEK 2
        public double? DaysWeek2Display => 5.0; // Mocking 5 for layout as per image

        // 24. TOTAL DAYS
        public double? TotalDaysDisplay => (DaysWeek1Display ?? 0) + (DaysWeek2Display ?? 0);

        // 25. HRS P/DAY
        public double? HrsPDayDisplay => 8.75;

        public bool HasSupervisorFee => Model?.IncentiveSupervisor > 0;

        // ----------------------------------------

        // Editable Properties (for main row)
        public decimal DeductionGas
        {
            get => Model?.DeductionGas ?? 0;
            set { if (Model != null && Model.DeductionGas != value) { Model.DeductionGas = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetPay)); } }
        }

        public decimal DeductionWashing
        {
            get => Model?.DeductionWashing ?? 0;
            set { if (Model != null && Model.DeductionWashing != value) { Model.DeductionWashing = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetPay)); } }
        }

        public decimal IncentiveSupervisor
        {
            get => Model?.IncentiveSupervisor ?? 0;
            set { if (Model != null && Model.IncentiveSupervisor != value) { Model.IncentiveSupervisor = value; OnPropertyChanged(); OnPropertyChanged(nameof(NetPay)); } }
        }

        public void RefreshTotalNett()
        {
            OnPropertyChanged(nameof(NetPay));
            OnPropertyChanged(nameof(TotalRem));
        }

        public decimal BasicNett => (Model?.TotalWage ?? 0) - ((Model?.DeductionLoan ?? 0) + (Model?.DeductionTax ?? 0) + (Model?.DeductionWashing ?? 0) + (Model?.DeductionGas ?? 0) + (Model?.DeductionOther ?? 0));
        public double VarianceHours => Model?.VarianceHours ?? 0;
        public decimal HourlyRate => Model?.HourlyRate ?? 0;
        public decimal TotalWage => Model?.TotalWage ?? 0;
        public double NormalHours => Model?.NormalHours ?? 0;
        public string Branch => Model?.Branch ?? string.Empty;
    }
}
