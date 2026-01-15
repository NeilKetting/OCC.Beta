using System;
using System.Collections.Generic;
using OCC.Shared.Enums;

namespace OCC.Shared.Models
{
    public class Incident : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public IncidentType Type { get; set; }
        public IncidentSeverity Severity { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportedByUserId { get; set; } = string.Empty;
        public IncidentStatus Status { get; set; } = IncidentStatus.Open;
        
        // Investigator
        public string InvestigatorId { get; set; } = string.Empty;

        // Findings
        public string RootCause { get; set; } = string.Empty;
        public string CorrectiveAction { get; set; } = string.Empty;

        // Collections
        public List<IncidentPhoto> Photos { get; set; } = new();

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
