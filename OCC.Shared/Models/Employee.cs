using System;

namespace OCC.Shared.Models
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // Date of Birth - Start with a default value of 30 years ago from today
        public DateTime DoB { get; set; } = DateTime.Now.AddYears(-30);
        public StaffRole Role { get; set; } = StaffRole.GeneralWorker;
        public double HourlyRate { get; set; }        
        public string DisplayName => $"{FirstName} {LastName}".Trim();
        public string EmployeeNumber { get; set; } = string.Empty;
        public EmploymentType EmploymentType { get; set; } = EmploymentType.Permanent;
        public IdType IdType { get; set; } = IdType.RSAId;
    }

    public enum EmploymentType
    {
        Permanent,
        Contract
    }

    public enum StaffRole
    {
        GeneralWorker,
        Builder,
        Tiler,
        Painter,
        Electrician,
        Plumber,
        Supervisor,
        ExternalContractor
    }

    public enum IdType
    {
        RSAId,
        Passport
    }
}
