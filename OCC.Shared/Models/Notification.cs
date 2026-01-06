using System;

namespace OCC.Shared.Models
{
    public class Notification : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Reminder;
        public string? TargetAction { get; set; }
        public Guid? UserId { get; set; }
    }

    public enum NotificationType
    {
        Reminder,
        Alert,
        Update,
        Message
    }
}
