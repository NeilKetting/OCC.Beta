using System;

namespace OCC.Shared.Models
{
    public class HseqTrainingRecord : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EmployeeName { get; set; } = string.Empty; // Should link to EmployeeId ideally
        public Guid? EmployeeId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string TrainingTopic { get; set; } = string.Empty;
        public DateTime DateCompleted { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Trainer { get; set; } = string.Empty;
        public string CertificateUrl { get; set; } = string.Empty;
        
        public string CertificateType { get; set; } = string.Empty; // e.g. First Aid L1
        public int ExpiryWarningDays { get; set; } = 30;

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
