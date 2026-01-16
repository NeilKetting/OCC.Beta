using System;

namespace OCC.Shared.Models
{
    public class WageRunLine : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid WageRunId { get; set; }
        // Navigation property if needed, but keeping it simple for now to avoid circular ref issues in serialization unless configured
        
        public Guid EmployeeId { get; set; }
        
        public string EmployeeName { get; set; } = string.Empty; // Snapshot name
        public string Branch { get; set; } = string.Empty;

        // Hours Breakdown
        public double NormalHours { get; set; }
        public double OvertimeHours { get; set; }
        
        // This is key for the "Advance Payment" logic
        public double ProjectedHours { get; set; } 
        
        // Adjustment from previous run (e.g. they were paid 9 projected hours but were absent)
        public double VarianceHours { get; set; } 
        
        public string VarianceNotes { get; set; } = string.Empty;

        // Financials
        public decimal HourlyRate { get; set; }
        public decimal TotalWage { get; set; }

        // IEntity Implementation
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
