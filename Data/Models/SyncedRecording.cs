using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Data.Models;

namespace backend.Data.Models;
[Table("SyncedRecordings")]
public class SyncedRecording
{
    [Key]
    [Required]
    [MaxLength(50)]
    public string Id { get; set; } = null!; // Recording ID from Clarify Go

    [MaxLength(50)]
    public string RecordingGroupID { get; set; } // Recording Group ID
    
    [Required]
    [MaxLength(512)] // Increased from implicit default to handle long URLs
    public string StreamingUrl { get; set; } = null!; // Azure Blob Storage URL

    [Required]
    [MaxLength(512)] // Increased from implicit default to handle long URLs
    public string DownloadUrl { get; set; } = null!; // Azure Blob Storage Download URL

    public bool IsDeleted { get; set; } // Is the recording deleted

    [Required]
    public DateTime RecordingDate { get; set; } // Date of the recording

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when added to DB

    public DateTime? DeletedAt { get; set; } // Timestamp when deleted

    [MaxLength(50)] // Matches SQL Size = 50
    public string Caller { get; set; } // Caller

    [MaxLength(50)] // Matches SQL Size = 50
    public string Callee { get; set; } // Callee

    public int DurationSeconds { get; set; } // Duration in seconds for easier storage

    [NotMapped] // Not stored in DB, computed property for JSON
    public string CallDuration => TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");

    public ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
}