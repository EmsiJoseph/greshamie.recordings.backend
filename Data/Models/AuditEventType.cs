using System.ComponentModel.DataAnnotations;

namespace backend.Data.Models
{
    public class AuditEventType
    {
        [Key] public int Id { get; set; }

        [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;
        [Required] [MaxLength(50)] public string NormalizedName { get; set; } = string.Empty;
        [Required] [MaxLength(100)] public string Description { get; set; } = string.Empty;
        public ICollection<AuditEvent> Events { get; set; } = new List<AuditEvent>();
    }
}