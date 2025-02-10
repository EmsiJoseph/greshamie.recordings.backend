using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Models
{
    public sealed class AuditEntry
    {
        [Key] public int Id { get; set; }

        [Required] public string UserId { get; set; } = string.Empty;

        [Required] public int EventId { get; set; }

        [Required] public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)] public string? Details { get; set; }

        [ForeignKey(nameof(UserId))] public User? User { get; set; }

        [ForeignKey(nameof(EventId))] public AuditEvent? Event { get; set; }
    }
}