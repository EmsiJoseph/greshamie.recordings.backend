using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("AuditEntries")]
    public class AuditEntry
    {
        [Key] public int Id { get; set; }

        [Required] public string UserId { get; set; } = string.Empty;

        [Required] public int EventId { get; set; }

        // Use UTC timestamp.
        [Required] public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)] public string? Details { get; set; }

        [ForeignKey(nameof(UserId))] public virtual User? User { get; set; }

        [ForeignKey(nameof(EventId))] public virtual AuditEvent? Event { get; set; }
    }
}