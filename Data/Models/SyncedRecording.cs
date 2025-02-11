using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("SyncedRecordings")]
    public class SyncedRecording
    {
        [Key]
        [Required]
        public string Id { get; set; } = null!;  // Recording ID from Clarify Go

        [Required]
        public string BlobUrl { get; set; } = null!; // Azure Blob Storage URL
        
        [Required]
        public string DownloadUrl { get; set; } = null!; // Azure Blob Storage Download URL
        
        [Required]
        public bool IsDeleted { get; set; } // Is the recording deleted
        
        [Required]
        public DateTime RecordingDate { get; set; } // Date of the recording

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp when added to DB
        
        public DateTime? DeletedAt { get; set; } // Timestamp when deleted
    }
}