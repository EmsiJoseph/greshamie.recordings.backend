using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Models
{
    [Table("SyncedRecordings")]
    public class SyncedRecording
    {
        [Key] [Required] [MaxLength(50)] public string Id { get; set; } = null!; // Recording ID from Clarify Go

        [Required] [MaxLength(100)] public string StreamingUrl { get; set; } = null!; // Azure Blob Storage URL

        [Required] [MaxLength(100)] public string DownloadUrl { get; set; } = null!; // Azure Blob Storage Download URL

        public bool IsDeleted { get; set; } // Is the recording deleted

        [Required] public DateTime RecordingDate { get; set; } // Date of the recording

        [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when added to DB

        public DateTime? DeletedAt { get; set; } // Timestamp when deleted
        public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
    }
}