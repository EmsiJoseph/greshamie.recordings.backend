using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Models
{
    public sealed class AuditEntry
    {
        [Key] public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required] [ForeignKey("Event")] public int EventId { get; set; }

        [ForeignKey("Recording")]
        [MaxLength(50)]
        public string? RecordId { get; set; }

        [Required] public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)] public string? Details { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public AuditEvent Event { get; set; } = null!;
        public SyncedRecording? Recording { get; set; }
    }
}