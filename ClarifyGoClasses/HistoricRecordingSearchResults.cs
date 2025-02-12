using System.Text.Json.Serialization;

namespace backend.ClarifyGoClasses
{
    // Represents the overall search results response.
    public class HistoricRecordingSearchResults
    {
        [JsonPropertyName("searchResults")]
        public List<HistoricRecordingSearchResult> SearchResults { get; set; } =
            new List<HistoricRecordingSearchResult>();

        [JsonPropertyName("totalResults")] public int TotalResults { get; set; }
    }

    // Represents an individual search result item.
    public class HistoricRecordingSearchResult
    {
        // The recording details.
        [JsonPropertyName("recording")]
        public HistoricRecordingRaw HistoricRecording { get; set; } = new HistoricRecordingRaw();

        // Count of screen recordings.
        [JsonPropertyName("screenRecordingCount")]
        public int ScreenRecordingCount { get; set; }

        // Count of tags.
        [JsonPropertyName("tagCount")] public int TagCount { get; set; }

        // Count of comments.
        [JsonPropertyName("commentCount")] public int CommentCount { get; set; }

        // Count of PCI events.
        [JsonPropertyName("pciEventCount")] public int PciEventCount { get; set; }

        // Count of recording evaluations.
        [JsonPropertyName("recordingEvaluationCount")]
        public int RecordingEvaluationCount { get; set; }
    }
}