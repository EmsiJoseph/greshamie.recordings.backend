using System.Text.Json.Serialization;

namespace backend.DTOs;

public class RecordingDto
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("caller")] public string? Caller { get; set; }
    [JsonPropertyName("receiver")] public string? Receiver { get; set; }
    [JsonPropertyName("startDateTime")] public DateTime? StartDateTime { get; set; }
    [JsonPropertyName("endDateTime")] public DateTime? EndDateTime { get; set; }
    [JsonPropertyName("callType")] public string? CallType { get; set; }
    [JsonPropertyName("isLive")] public bool? IsLive { get; set; }
    [JsonPropertyName("durationSeconds")] public int? DurationSeconds { get; set; }
    [JsonPropertyName("recorder")] public string? Recorder { get; set; }
}