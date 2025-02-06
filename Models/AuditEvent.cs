using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class AuditEvent
    {
        [Key] public int Id { get; set; }

        [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;

        [Required] [MaxLength(100)] public string Description { get; set; } = string.Empty;
    }
}