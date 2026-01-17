using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents a daily attendance entry for an employee or user.
    /// Tracks clock-in/out times, geolocation, and general availability status.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>AttendanceRecords</c> table.
    /// <b>How:</b> Entries are generated either by the manual attendance sheet (Office) 
    /// or via the mobile app "Clock-In" feature. Geolocation data is used to verify site presence.
    /// </remarks>
    public class AttendanceRecord : IEntity
    {
        /// <summary> Unique primary key for the attendance record. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary> Optional foreign key to the <see cref="User"/> account (system login). </summary>
        public Guid? UserId { get; set; }

        /// <summary> Foreign key to the <see cref="Employee"/> record. </summary>
        public Guid? EmployeeId { get; set; }
        
        /// <summary> The calendar date of the attendance entry. </summary>
        public DateTime Date { get; set; }

        /// <summary> The category of presence (Present, Absent, Sick, etc.). </summary>
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        
        /// <summary> GPS Latitude at the time of check-in/checkout. </summary>
        public double? Latitude { get; set; }

        /// <summary> GPS Longitude at the time of check-in/checkout. </summary>
        public double? Longitude { get; set; }
        
        /// <summary> Full timestamp of the first check-in of the day. </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary> Full timestamp of the final check-out of the day. </summary>
        public DateTime? CheckOutTime { get; set; }
        
        /// <summary> Total calculated working hours for the day. </summary>
        public double HoursWorked { get; set; }

        /// <summary> General comments or descriptions for the day. </summary>
        public string? Notes { get; set; }

        /// <summary> Explanation for absence or leaving early. </summary>
        public string? LeaveReason { get; set; }

        /// <summary> Remote path or identifier for an uploaded medical certificate. </summary>
        public string? DoctorsNoteImagePath { get; set; }

        /// <summary> The branch where the attendance was registered. </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary> The specific time the employee clocked in (snapshot). </summary>
        public TimeSpan? ClockInTime { get; set; }
        
        /// <summary> 
        /// Snapshot of the employee's hourly rate at the time this record was created.
        /// Essential for historical payroll accuracy if rates change later.
        /// </summary>
        public decimal? CachedHourlyRate { get; set; }

        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Record creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last modification timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Defines various states of daily presence.
    /// </summary>
    public enum AttendanceStatus
    {
        /// <summary> Employee is on site and working. </summary>
        Present,
        /// <summary> Employee did not arrive and has no prior authorization. </summary>
        Absent,
        /// <summary> Employee arrived after the scheduled shift start. </summary>
        Late,
        /// <summary> Employee is off duty due to illness. </summary>
        Sick,
        /// <summary> Employee left the site before the scheduled shift end. </summary>
        LeaveEarly,
        /// <summary> Employee is off duty with prior management approval. </summary>
        LeaveAuthorized
    }
}

