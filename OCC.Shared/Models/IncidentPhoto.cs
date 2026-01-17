using System;

namespace OCC.Shared.Models
{
    /// <summary>
    /// Represents photographic evidence attached to an <see cref="Incident"/> report.
    /// </summary>
    /// <remarks>
    /// <b>Where:</b> Persisted in the <c>IncidentPhotos</c> table (or related storage).
    /// <b>How:</b> Linked to a parent <see cref="Incident"/>. Stores the image data (or reference) and a caption.
    /// </remarks>
    public class IncidentPhoto : IEntity
    {
        /// <summary> Unique primary key for the photo record. </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary> Foreign Key linking to the parent <see cref="Incident"/>. </summary>
        public Guid IncidentId { get; set; }

        /// <summary> The image data encoded as a Base64 string. </summary>
        public string Base64Content { get; set; } = string.Empty;

        /// <summary> A description or caption for the photo (e.g., "Damage to left fender"). </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary> Timestamp when the photo was uploaded/captured. </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Soft-delete flag. </summary>
        public bool IsDeleted { get; set; }

        /// <summary> Creation timestamp. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary> Last modification timestamp. </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
