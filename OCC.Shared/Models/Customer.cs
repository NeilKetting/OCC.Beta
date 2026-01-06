using System;

namespace OCC.Shared.Models
{
    public class Customer : IEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty; // For grouping if needed
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
