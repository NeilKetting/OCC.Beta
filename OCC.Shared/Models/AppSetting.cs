using System;
using System.ComponentModel.DataAnnotations;

namespace OCC.Shared.Models
{
    public class AppSetting : IEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}
