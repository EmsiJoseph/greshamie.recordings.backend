namespace backend.DTOs.Recording;

public class RecordingDto
{
    public string? Id { get; set; }
    public string? Caller { get; set; }
    public string? Receiver { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? CallType { get; set; }
    public bool? IsLive { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Recorder { get; set; }
}