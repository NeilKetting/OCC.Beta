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
    public class WageRunLine : IEntity
    {
        /// <summary> Unique primary key for the wage line item. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary> Foreign Key to the parent <see cref="WageRun"/>. </summary>
        public Guid WageRunId { get; set; }
        
        /// <summary> Foreign Key to the <see cref="Employee"/> being paid. </summary>
        public Guid EmployeeId { get; set; }
        
        /// <summary> Snapshot of the employee's name at time of generation. </summary>
        public string EmployeeName { get; set; } = string.Empty; 

        /// <summary> The branch where the employee is based. </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary> Total standard working hours verified for this period. </summary>
        public double NormalHours { get; set; }

        /// <summary> Total authorized overtime hours. </summary>
        public double OvertimeHours { get; set; }
        
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
        
        /// <summary> Explanation for any <see cref="VarianceHours"/> applied. </summary>
        public string VarianceNotes { get; set; } = string.Empty;

        /// <summary> The hourly pay rate applied (Snapshot). </summary>
        public decimal HourlyRate { get; set; }

        /// <summary> The final calculated wage amount (Hours * Rate). </summary>
        public decimal TotalWage { get; set; }

        // IEntity Implementation
        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last updated timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
