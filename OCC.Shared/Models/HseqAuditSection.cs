using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a specific category or section within an HSEQ Audit (e.g., "PPE", "Electrical Safety").
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>HseqAuditSections</c> table.
    /// <b>How:</b> Sections aggregate scores for specific safety categories within a parent <see cref="HseqAudit"/>.
    /// </remarks>
    public class HseqAuditSection : IEntity
    {
        /// <summary> Unique primary key for the audit section. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary> Foreign key linking this section to a parent <see cref="HseqAudit"/>. </summary>
        public Guid AuditId { get; set; }

        /// <summary> The name of the category (e.g., "Public Safety", "Tool Maintenance"). </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> The maximum decimal score achievable in this category. </summary>
        public decimal PossibleScore { get; set; }

        /// <summary> The actual achieved score for this section. </summary>
        public decimal ActualScore { get; set; }
        
        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Section creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last modification timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

