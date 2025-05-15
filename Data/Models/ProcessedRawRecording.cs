using System.Text.Json.Serialization;
using backend.ClarifyGoClasses;

namespace backend.Data.Models;

public class ProcessedRawRecording
{
    [JsonPropertyName("recording")]
    public ProcessedRecording Recording { get; set; } = null!;

    [JsonPropertyName("screenRecordingCount")]
    public int ScreenRecordingCount { get; set; }

    [JsonPropertyName("tagCount")]
    public int TagCount { get; set; }

    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    [JsonPropertyName("pciEventCount")]
    public int PciEventCount { get; set; }

    [JsonPropertyName("recordingEvaluationCount")]
    public int RecordingEvaluationCount { get; set; }
}

public class ProcessedRecording : HistoricRecordingRaw
{
    [JsonPropertyName("callingPartyNumber")]
    public string? CallingPartyNumber { get; set; }

    [JsonPropertyName("calledPartyNumber")]
    public string? CalledPartyNumber { get; set; }
}