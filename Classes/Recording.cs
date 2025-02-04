namespace backend.Models;

public class Recording
{
    public int Id { get; set; }
    public string Caller { get; set; }
    public string Receiver { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int CallTypeId { get; set; }
    public bool IsLive { get; set; }
    public int Duration { get; set; }
    public string Recorder { get; set; }
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
 
}