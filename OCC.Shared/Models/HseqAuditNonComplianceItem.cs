using System;
using OCC.Shared.Enums;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a detected failure or deviation from safety standards during an audit.
    /// Tracks the issue, regulatory reference, and the corrective action plan.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>HseqAuditNonComplianceItems</c> table.
    /// <b>How:</b> Linked to an <see cref="HseqAudit"/>. Must be tracked until <see cref="Status"/> is Closed.
    /// </remarks>
    public class HseqAuditNonComplianceItem : IEntity
    {
        /// <summary> Unique primary key for the non-compliance item. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary> Foreign Key linking to the parent <see cref="HseqAudit"/>. </summary>
        public Guid AuditId { get; set; }

        /// <summary> Detailed description of the deviation or unsafe condition. </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary> Reference to the specific regulation violated. </summary>
        public string RegulationReference { get; set; } = string.Empty;

        /// <summary> Base64 encoded string of photo evidence showing the non-compliance. </summary>
        public string PhotoBase64 { get; set; } = string.Empty;
        
        // Deviation Action Sheet Fields
        
        /// <summary> The required steps to rectify the issue. </summary>
        public string CorrectiveAction { get; set; } = string.Empty;

        /// <summary> Name of the individual assigned to fix the issue. </summary>
        public string ResponsiblePerson { get; set; } = string.Empty;

        /// <summary> Deadline for resolving the non-compliance. </summary>
        public DateTime? TargetDate { get; set; }

        /// <summary> The actual date the issue was resolved. </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary> Current resolution status (Open, Closed). </summary>
        public AuditItemStatus Status { get; set; } = AuditItemStatus.Open;

        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last modification timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
