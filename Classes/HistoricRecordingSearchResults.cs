using System.Text.Json.Serialization;

namespace backend.Classes
{
    // Represents the overall search results response.
    public class RecordingSearchResults
    {
        [JsonPropertyName("searchResults")]
        public List<RecordingSearchResult> SearchResults { get; set; } = new List<RecordingSearchResult>();
    }

    // Represents an individual search result item.
    public class RecordingSearchResult
    {
        // The recording details.
        [JsonPropertyName("recording")]
        public ClarifyGoRecordingRaw Recording { get; set; } = new ClarifyGoRecordingRaw();

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