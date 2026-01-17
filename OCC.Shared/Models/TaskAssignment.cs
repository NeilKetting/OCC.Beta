using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents the assignment of a resource (Employee) to a specific <see cref="ProjectTask"/>.
    /// Allows for multiple people to be assigned to a single task.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>TaskAssignments</c> table.
    /// <b>How:</b> Links a <see cref="ProjectTask"/> to an <see cref="Employee"/> (Assignee). 
    /// Includes the assignee's name as a snapshot for quick display.
    /// </remarks>
    public class TaskAssignment : IEntity
    {
        /// <summary> Unique primary key for the assignment record. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary> Foreign Key to the <see cref="ProjectTask"/>. </summary>
        public Guid TaskId { get; set; }

        /// <summary> Navigation property to the assigned task. </summary>
        public ProjectTask ProjectTask { get; set; } = null!;

        /// <summary> Foreign Key of the assigned resource (e.g., Employee ID). </summary>
        public Guid AssigneeId { get; set; }

        /// <summary> Classification of the assignee (currently only "Staff"). </summary>
        public AssigneeType AssigneeType { get; set; }

        /// <summary> Cached display name of the assignee (Employee Name). </summary>
        public string AssigneeName { get; set; } = string.Empty; 
    }

    /// <summary>
    /// Takes the type of resource being assigned.
    /// </summary>
    public enum AssigneeType
    {
        /// <summary> Valid Employee of the company. </summary>
        Staff
    }
}
