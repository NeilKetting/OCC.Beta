namespace OCC.Shared.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed, OnHold
        public string ProjectManager { get; set; } = string.Empty;
        public Guid? SiteManagerId { get; set; }
        public virtual Employee? SiteManager { get; set; }
        public string Customer { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string ShortName { get; set; } = string.Empty;

        // Work Hours Snapshot
        public TimeSpan WorkStartTime { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan WorkEndTime { get; set; } = new TimeSpan(17, 0, 0);
        public int LunchDurationMinutes { get; set; } = 60;

        // Navigation Properties
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        
        public Guid? CustomerId { get; set; }
        public virtual Customer? CustomerEntity { get; set; }
    }
}
