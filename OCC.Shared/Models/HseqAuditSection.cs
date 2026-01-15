using System;

namespace OCC.Shared.Models
{
    public class HseqAuditSection : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AuditId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PossibleScore { get; set; }
        public decimal ActualScore { get; set; }
        
        // e.g. "Public Safety", "PPE", etc.
        
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
