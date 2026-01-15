using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace OCC.Shared.Models
{
    public class BugComment : IEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid BugReportId { get; set; }
        
        public string AuthorName { get; set; } = string.Empty;
        
        public string AuthorEmail { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsDevComment { get; set; }

        // Helper properties for UI
        public string Initials => !string.IsNullOrEmpty(AuthorName) ? 
            string.Join("", AuthorName.Split(' ').Where(s => !string.IsNullOrEmpty(s)).Select(n => n[0])).ToUpper() : "??";
    }
}
