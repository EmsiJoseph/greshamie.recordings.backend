namespace backend.DTOs;

public class RecordingDto
{
    public string? id { get; set; }
    public string? caller { get; set; }
    public string? receiver { get; set; }
    public DateTime? startDateTime { get; set; }
    public DateTime? endDateTime { get; set; }
    public string? callType { get; set; }
    public bool? isLive { get; set; }
    public int? durationSeconds { get; set; }
    public string? recorder { get; set; }
    public bool? hasPciCompliance { get; set; }
    public bool? hasVideoRecording { get; set; }
    public bool? hasQualityEvaluation { get; set; }
}