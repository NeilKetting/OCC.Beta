using System;

namespace OCC.Shared.Models
{
    public class IncidentPhoto : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid IncidentId { get; set; }
        public string Base64Content { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
