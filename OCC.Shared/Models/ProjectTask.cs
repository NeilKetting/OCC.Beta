using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using OCC.Shared.Enums;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a specific unit of work or activity within a <see cref="Project"/>.
    /// Supports hierarchical structures (Gantt chart style), dependencies, and resource assignment.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>ProjectTasks</c> table.
    /// <b>How:</b> Tasks belong to a <see cref="Project"/> and can be grouped via <see cref="ParentId"/> / <see cref="Children"/>.
    /// Time tracked against tasks feeds into project costing.
    /// </remarks>
    public class ProjectTask : BaseEntity, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        /// <summary>
        /// The user who owns this task (if it's a personal task). 
        /// For project tasks, this might typically be the creator or null.
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary> Optional ID from external systems like MS Project or Gantt exports. </summary>
        public string? LegacyId { get; set; }

        /// <summary> The name or title of the task. </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> Scheduled start date. </summary>
        public DateTime StartDate { get; set; }

        /// <summary> Scheduled completion date. </summary>
        public DateTime FinishDate { get; set; }
        
        /// <summary> Textual representation of duration (e.g., "5 days"). </summary>
        public string Duration { get; set; } = string.Empty;

        /// <summary> Progress percentage (0-100). </summary>
        public int PercentComplete { get; set; }

        /// <summary> Priority level (Low, Medium, High, Critical). Default is "Medium". </summary>
        public string Priority { get; set; } = "Medium";

        #region Reminders
        /// <summary> Whether a reminder is active for this task. </summary>
        public bool IsReminderSet { get; set; }

        /// <summary> How often the reminder should repeat. </summary>
        public ReminderFrequency Frequency { get; set; } = ReminderFrequency.Once;

        /// <summary> The date/time of the next reminder. </summary>
        public DateTime? NextReminderDate { get; set; }
        #endregion

        /// <summary> Status string (e.g., "To Do", "In Progress", "Completed"). </summary>
        public string Status { get; set; } = "To Do";

        /// <summary> Helper property indicating if the task is finished. </summary>
        public bool IsComplete => Status == "Completed" || PercentComplete == 100;

        /// <summary> Detailed description or instructions for the task. </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary> The category of the activity (Task vs Meeting). </summary>
        public TaskType Type { get; set; } = TaskType.Task;

        /// <summary> Gets a comma-separated string of initials for all assigned staff. </summary>
        public string AssigneeInitials => Assignments == null || !Assignments.Any() 
            ? "--" 
            : string.Join(", ", Assignments.Select(a => string.Concat(a.AssigneeName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]))).Select(s => s.ToUpper()));

        /// <summary> User comments and discussion thread attached to this task. </summary>
        public List<TaskComment> Comments { get; set; } = new();

        /// <summary> Resources (employees/teams) assigned to this task. </summary>
        public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        
        /// <summary> If true, work is temporarily suspended. </summary>
        public bool IsOnHold { get; set; }

        /// <summary> Foreign Key to the parent <see cref="Models.Project"/>. Nullable for Personal Tasks. </summary>
        public Guid? ProjectId { get; set; }
        
        /// <summary> Navigation property to the parent Project. </summary>
        public virtual Project? Project { get; set; }
        
        /// <summary> Navigation property to the parent task (if any). </summary>
        public virtual ProjectTask? ParentTask { get; set; }

        /// <summary> Optional Foreign Key to a parent task (for nested sub-tasks). </summary>
        public Guid? ParentId { get; set; }

        /// <summary> Collection of sub-tasks. </summary>
        public List<ProjectTask> Children { get; set; } = new();

        /// <summary> List of dependency task IDs (Predecessors). </summary>
        public List<string> Predecessors { get; set; } = new();

        /// <summary> The visual sort order index for display. </summary>
        public int OrderIndex { get; set; }

        /// <summary> visual indentation level for hierarchical display (0 is root). </summary>
        public int IndentLevel { get; set; }

        /// <summary> If true, this is a summary/container task with children. </summary>
        public bool IsGroup { get; set; }
        
        // UI Helpers
        private bool _isExpanded = true;
        
        /// <summary> UI State: whether the sub-tasks are visible. </summary>
        public bool IsExpanded 
        { 
            get => _isExpanded; 
            set 
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary> Helper to check if the task is a parent node. </summary>
        public bool HasChildren => Children != null && Children.Any();

        #region Actuals
        /// <summary> The actual date work commenced. </summary>
        public DateTime? ActualStartDate { get; set; }
        /// <summary> The actual date work was completed. </summary>
        public DateTime? ActualCompleteDate { get; set; }
        /// <summary> The actual time taken to complete the task. </summary>
        public TimeSpan? ActualDuration { get; set; }
        /// <summary> Planned duration in hours (stored as nullable TimeSpan). </summary>
        public TimeSpan? PlannedDurationHours { get; set; }

        /// <summary> Legacy property to support old database schema. </summary>
        public string AssignedTo { get; set; } = string.Empty;

        #endregion

        #region Geofencing
        /// <summary> GPS Latitude for verifying task location (if applicable). </summary>
        public double? Latitude { get; set; }
        /// <summary> GPS Longitude for verifying task location (if applicable). </summary>
        public double? Longitude { get; set; }
        #endregion
    }

    /// <summary>
    /// Differentiates between standard work tasks and scheduled meetings.
    /// </summary>
    public enum TaskType
    {
        /// <summary> A standard work activity. </summary>
        Task,
        /// <summary> A scheduled meeting or consultation. </summary>
        Meeting,
        /// <summary> A quick personal to-do item. </summary>
        PersonalToDo
    }
}
