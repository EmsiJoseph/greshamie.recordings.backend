namespace backend.Models;

public class AuditEntry
{
    public Guid Id { get; set; }
    public string UserId { get; set; } // From ClarifyGo's token
    public string Action { get; set; } // e.g., "DownloadRecording"
    public DateTime Timestamp { get; set; }
    public string ResourceId { get; set; } // Recording ID
    public string ClientIp { get; set; }
}