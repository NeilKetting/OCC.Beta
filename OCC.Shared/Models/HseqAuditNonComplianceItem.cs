using System;
using OCC.Shared.Enums;

namespace OCC.Shared.Models
{
    public class HseqAuditNonComplianceItem : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AuditId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RegulationReference { get; set; } = string.Empty;
        public string PhotoBase64 { get; set; } = string.Empty;
        
        // Deviation Action Sheet Fields
        public string CorrectiveAction { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public AuditItemStatus Status { get; set; } = AuditItemStatus.Open;

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
