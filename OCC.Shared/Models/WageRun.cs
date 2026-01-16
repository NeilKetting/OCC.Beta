using System;
using System.Collections.Generic;

namespace OCC.Shared.Models
{
    public class WageRun : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RunDate { get; set; } = DateTime.UtcNow;
        
        public WageRunStatus Status { get; set; } = WageRunStatus.Draft;
        public string? Notes { get; set; }
        
        public List<WageRunLine> Lines { get; set; } = new();

        // IEntity Implementation
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public enum WageRunStatus
    {
        Draft,
        Finalized,
        Paid
    }
}
