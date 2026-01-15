using System;

namespace OCC.Shared.Models
{
    public class HseqDocument : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public Enums.DocumentCategory Category { get; set; } = Enums.DocumentCategory.Other;
        public string FilePath { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
        public string FileSize { get; set; } = "0 KB";

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
