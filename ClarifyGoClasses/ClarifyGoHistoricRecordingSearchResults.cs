using System.Text.Json.Serialization;

namespace backend.Classes
{
    // Represents the overall search results response.
    public class HistoricRecordingSearchResults
    {
        [JsonPropertyName("searchResults")]
        public List<ClarifyGoHistoricRecordingSearchResult> SearchResults { get; set; } = new List<ClarifyGoHistoricRecordingSearchResult>();
        
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }
    }

    // Represents an individual search result item.
    public class ClarifyGoHistoricRecordingSearchResult
    {
        // The recording details.
        [JsonPropertyName("recording")]
        public ClarifyGoHistoricRecordingRaw HistoricRecording { get; set; } = new ClarifyGoHistoricRecordingRaw();

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