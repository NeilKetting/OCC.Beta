/// <summary>
/// Represents a record of a user dismissing a specific notification/entity.
/// Used to prevent reappearance of "active" notifications (like pending approvals) 
/// that the user has chosen to ignore.
/// </summary>
using System;
using System.ComponentModel.DataAnnotations;

namespace OCC.Shared.Models
{
    public class NotificationDismissal
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The ID of the User who dismissed the notification.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The unique ID of the entity associated with the notification (e.g., UserId for registration, RequestId for Leave).
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// The type of notification/entity (e.g., "UserRegistration", "LeaveRequest", "Birthday").
        /// </summary>
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// When the dismissal occurred.
        /// </summary>
        public DateTime DismissedAt { get; set; } = DateTime.UtcNow;
    }
}
