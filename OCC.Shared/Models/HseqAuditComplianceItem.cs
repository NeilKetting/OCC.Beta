using System;

namespace OCC.Shared.Models
{
    public class HseqAuditComplianceItem : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AuditId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string RegulationReference { get; set; } = string.Empty;
        public string PhotoBase64 { get; set; } = string.Empty;
        
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
