using System;

namespace OCC.Shared.DTOs
{
    public class ProjectSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string ProjectManager { get; set; } = string.Empty;
        public int Progress { get; set; }
        public DateTime? LatestFinish { get; set; }
        public int TaskCount { get; set; }
    }
}
