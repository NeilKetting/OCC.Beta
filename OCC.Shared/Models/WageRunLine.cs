using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a single employee's wage calculation within a specific <see cref="WageRun"/>.
    /// Captures the breakdown of hours, rates, and final payment amount.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>WageRunLines</c> table.
    /// <b>How:</b> Calculated based on <see cref="AttendanceRecord"/> data for the period. 
    /// Includes logic for handling "Projected Hours" (advance payment) and correcting variances from previous runs.
    /// </remarks>
    public class WageRunLine : BaseEntity
    {

        
        /// <summary> Foreign Key to the parent <see cref="WageRun"/>. </summary>
        public Guid WageRunId { get; set; }
        
        /// <summary> Foreign Key to the <see cref="Employee"/> being paid. </summary>
        public Guid EmployeeId { get; set; }
        
        /// <summary> Snapshot of the employee's name at time of generation. </summary>
        public string EmployeeName { get; set; } = string.Empty; 

        /// <summary> The branch where the employee is based. </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary> Number of days worked or credited in Week 1. </summary>
        public double DaysWeek1 { get; set; }

        /// <summary> Number of days worked or credited in Week 2. </summary>
        public double DaysWeek2 { get; set; }

        /// <summary> Standard Hours per Day (typically 8.75). </summary>
        public double HoursPerDay { get; set; } = 8.75;

        /// <summary> Calculated Rate Per Day. </summary>
        public decimal RatePerDay { get; set; }

        /// <summary> Calculated Rate Per Hour (base hourly rate). </summary>
        public decimal RatePerHour { get; set; }

        /// <summary> Calculated Standard Overtime Rate (1.5x). </summary>
        public decimal StdOvertimeRate { get; set; }

        /// <summary> Calculated Saturday Overtime Rate (1.5x). </summary>
        public decimal SatOvertimeRate { get; set; }

        /// <summary> Calculated Sunday Overtime Rate (2.0x). </summary>
        public decimal SunOvertimeRate { get; set; }

        /// <summary> Calculated December Rate. </summary>
        public decimal DecRate { get; set; }

        /// <summary> Standard Overtime hours (Weekday). </summary>
        public double StdOvertime { get; set; }

        /// <summary> Saturday Overtime hours. </summary>
        public double SatOvertime { get; set; }

        /// <summary> Sunday Overtime hours. </summary>
        public double SunOvertime { get; set; }

        /// <summary> Total standard working hours verified for this period. </summary>
        public double NormalHours { get; set; }

        /// <summary> Total authorized overtime hours (Saturday or Weekday Late @ 1.5x). </summary>
        public double Overtime15Hours { get; set; }
        
        /// <summary> Total authorized overtime hours (Sunday or Public Holiday @ 2.0x). </summary>
        public double Overtime20Hours { get; set; }
        
        /// <summary> 
        /// Hours estimated for future work within this pay cycle (used for Advance Payments).
        /// e.g., Paying for Friday on a Thursday run.
        /// </summary>
        public double ProjectedHours { get; set; } 
        
        /// <summary> 
        /// Correction hours from previous periods.
        /// e.g., If an employee was paid for 9 projected hours last week but was absent, this will be negative to recoup costs.
        /// </summary>
        public double VarianceHours { get; set; } 

        /// <summary> Total unpaid lunch hours deducted during the period (12:00-13:00 slot). </summary>
        public double LunchDeductionHours { get; set; } 
        
        /// <summary> Explanation for any <see cref="VarianceHours"/> applied. </summary>
        public string VarianceNotes { get; set; } = string.Empty;

        /// <summary> The hourly pay rate applied (Snapshot). </summary>
        public decimal HourlyRate { get; set; }

        /// <summary> The final calculated wage amount (Hours * Rate). </summary>
        public decimal TotalWage { get; set; }

        /// <summary> Amount deducted for loan repayments. </summary>
        public decimal DeductionLoan { get; set; }

        /// <summary> Amount deducted for tax/PAYE/UIF. </summary>
        public decimal DeductionTax { get; set; }

        /// <summary> Other deductions (e.g., washing, damages). </summary>
        public decimal DeductionOther { get; set; }

        /// <summary> Amount deducted for Washing. </summary>
        public decimal DeductionWashing { get; set; }

        /// <summary> Amount deducted for Gas. </summary>
        public decimal DeductionGas { get; set; }

        /// <summary> Employee's Bank Name. </summary>
        public string? BankName { get; set; }

        /// <summary> Employee's Bank Account Number (shown as Comments). </summary>
        public string? AccountNumber { get; set; }

        /// <summary> December Hours. </summary>
        public double DecHrs { get; set; }

        /// <summary> December Total. </summary>
        public decimal DecTotal { get; set; }

        /// <summary> Supervisor incentive fee (e.g., R500). </summary>
        public decimal IncentiveSupervisor { get; set; }

        /// <summary> 
        /// Final payout amount: TotalWage + Incentives - Deductions. 
        /// </summary>
        public decimal NetPay => (TotalWage + IncentiveSupervisor) - (DeductionLoan + DeductionTax + DeductionOther + DeductionWashing + DeductionGas);

        // IEntity Implementation - Replaced by BaseEntity

    }
}
