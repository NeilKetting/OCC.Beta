using System;
using System.Collections.Generic;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a payroll calculation period (e.g., Weekly or Bi-Weekly wages).
    /// Aggregates attendance and rate data to determine amounts due to employees.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>WageRuns</c> table.
    /// <b>How:</b> Contains multiple <see cref="WageRunLine"/> entries, one per employee. 
    /// Can be in Draft state for review before being Finalized.
    /// </remarks>
    public class WageRun : IEntity
    {
        /// <summary> Unique primary key for the wage run. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary> The start date of the pay period (inclusive). </summary>
        public DateTime StartDate { get; set; }

        /// <summary> The end date of the pay period (inclusive). </summary>
        public DateTime EndDate { get; set; }

        /// <summary> The date this wage run was generated. </summary>
        public DateTime RunDate { get; set; } = DateTime.UtcNow;
        
        /// <summary> Current state: Draft, Finalized, or Paid. </summary>
        public WageRunStatus Status { get; set; } = WageRunStatus.Draft;

        /// <summary> Administrative notes regarding this pay run. </summary>
        public string? Notes { get; set; }
        
        /// <summary> Collection of individual employee wage calculations for this run. </summary>
        public List<WageRunLine> Lines { get; set; } = new();

        // IEntity Implementation
        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last updated timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Status of the payroll processing workflow.
    /// </summary>
    public enum WageRunStatus
    {
        /// <summary> Initial calculation, open for editing and review. </summary>
        Draft,
        /// <summary> Confirmed and locked. No further changes allowed. </summary>
        Finalized,
        /// <summary> Payment has been processed/disbursed. </summary>
        Paid
    }
}
