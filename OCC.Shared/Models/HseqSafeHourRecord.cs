using System;

namespace OCC.Shared.Models
{
    public class HseqSafeHourRecord : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Month { get; set; }
        public double SafeWorkHours { get; set; }
        public string IncidentReported { get; set; } = string.Empty; // "Yes", "No", or Incident ID
        public int NearMisses { get; set; }
        public string RootCause { get; set; } = string.Empty;
        public string CorrectiveActions { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public string ReportedBy { get; set; } = string.Empty;

        // IEntity Implementation
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
