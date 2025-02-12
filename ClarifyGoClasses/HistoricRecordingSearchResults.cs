using System.Text.Json.Serialization;

namespace backend.ClarifyGoClasses
{
    // Represents the overall search results response.
    public class ClarifyGoHistoricRecordingSearchResults
    {
        public List<ClarifyGoHistoricRecordingSearchResult> SearchResults { get; set; } =
            new List<ClarifyGoHistoricRecordingSearchResult>();

        public int TotalResults { get; set; }
    }

    // Represents an individual search result item.
    public class ClarifyGoHistoricRecordingSearchResult
    {
        // The recording details.

        public HistoricRecordingRaw HistoricRecording { get; set; } = new HistoricRecordingRaw();

        // Count of screen recordings.

        public int ScreenRecordingCount { get; set; }

        // Count of tags.
        public int TagCount { get; set; }

        // Count of comments.
        public int CommentCount { get; set; }

        // Count of PCI events.
        public int PciEventCount { get; set; }

        // Count of recording evaluations.

        public int RecordingEvaluationCount { get; set; }
    }
}