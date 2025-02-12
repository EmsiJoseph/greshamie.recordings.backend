using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models;

namespace backend.Data.Models
{
    public sealed class AuditEntry
    {
        [Key] public int Id { get; set; }

        [Required] public string UserId { get; set; } = string.Empty;

        [Required] public int EventId { get; set; }
        
        [Required] public string RecordId { get; set; } = string.Empty;
        [Required] public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)] public string? Details { get; set; }

        [ForeignKey(nameof(UserId))] public User? User { get; set; }

        [ForeignKey(nameof(EventId))] public AuditEvent? Event { get; set; }
        [ForeignKey(nameof(RecordId))] public SyncedRecording? Recording { get; set; }
    }
}