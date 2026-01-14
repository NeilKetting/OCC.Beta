using System;

namespace OCC.Shared.Models
{
    public class HseqAudit : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public string SiteName { get; set; } = string.Empty;
        public string ScopeOfWorks { get; set; } = string.Empty;
        public string SiteManager { get; set; } = string.Empty; // Could link to EmployeeId potentially
        public string SiteSupervisor { get; set; } = string.Empty;
        public string HseqConsultant { get; set; } = string.Empty;
        public string AuditNumber { get; set; } = string.Empty;
        public decimal TargetScore { get; set; }
        public decimal ActualScore { get; set; }
        public string Findings { get; set; } = string.Empty;
        public string NonConformance { get; set; } = string.Empty;
        public string ImmediateAction { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public DateTime? CloseOutDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
