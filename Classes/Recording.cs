namespace backend.Models;

public class Recording
{
    public string? Id { get; set; }
    public string? Caller { get; set; }
    public string? Receiver { get; set; }
    public DateTime? FromDateTime { get; set; }
    public DateTime? ToDateTime { get; set; }
    public int CallTypeId { get; set; }
    public bool IsLive { get; set; }
    public int DurationSeconds { get; set; }
    public string? Recorder { get; set; }
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
 
}