namespace OCC.Shared.Models
{
    public class ProjectTask : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? LegacyId { get; set; } // For MSP or other string IDs
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string Duration { get; set; } = string.Empty;
        public int PercentComplete { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "To Do";
        public bool IsComplete => Status == "Completed" || PercentComplete == 100;
        public string AssignedTo { get; set; } = "UN"; // Unassigned
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; } = TaskType.Task;
        public List<TaskComment> Comments { get; set; } = new();
        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public bool IsOnHold { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        
        public List<ProjectTask> Children { get; set; } = new();
        public List<string> Predecessors { get; set; } = new();
        public int OrderIndex { get; set; }
        public int IndentLevel { get; set; }
        public bool IsGroup { get; set; } // Renamed from IsSummary to avoid confusion with MSP Summary property if exists

        /// <summary>
        /// Gets or sets the actual date and time when the task was completed.
        /// </summary>
        #region Actuals
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompleteDate { get; set; }
        public TimeSpan? ActualDuration { get; set; }
        public TimeSpan? PlanedDurationHours { get; set; } // Added for compatibility with TaskItem logic
        #endregion

        #region Geofencing
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        #endregion
    }

    public enum TaskType
    {
        Task,
        Meeting
    }
}
