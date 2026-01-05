namespace OCC.Shared.Models
{
    public class TaskAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ProjectTaskId { get; set; }
        public ProjectTask ProjectTask { get; set; } = null!;

        public Guid AssigneeId { get; set; }
        public AssigneeType AssigneeType { get; set; }
        public string AssigneeName { get; set; } = string.Empty; // Cached name for display
    }

    public enum AssigneeType
    {
        Staff
    }
}
