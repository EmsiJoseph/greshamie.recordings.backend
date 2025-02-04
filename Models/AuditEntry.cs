namespace backend.Models;

public class AuditEntry
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Action { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Details { get; set; }
}