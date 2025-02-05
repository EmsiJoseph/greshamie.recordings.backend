using System;
using System.IO;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services.HistoricRecordings
{
    public interface IHistoricRecordingsService
    {
        /// <summary>
        /// Searches for historic recordings based on the specified date range and filter criteria.
        /// </summary>
        /// <param name="startDate">The start date for the search period.</param>
        /// <param name="endDate">The end date for the search period.</param>
        /// <param name="filters">Optional frontend-friendly filter criteria.</param>
        /// <returns>A <see cref="RecordingSearchResults"/> object containing the search results.</returns>
        Task<RecordingSearchResults> SearchRecordingsAsync(
            DateTime startDate,
            DateTime endDate,
            RecordingSearchFilters filters = null);

        /// <summary>
        /// Deletes a recording specified by its ID.
        /// </summary>
        Task DeleteRecordingAsync(string recordingId);

        /// <summary>
        /// Exports the specified recording as an MP3.
        /// </summary>
        Task<Stream> ExportMp3Async(string recordingId);

        /// <summary>
        /// Exports the specified recording as a WAV.
        /// </summary>
        Task<Stream> ExportWavAsync(string recordingId);
    }

    /// <summary>
    /// Represents the search filters using frontend-friendly property names.
    /// </summary>
    public class RecordingSearchFilters
    {
        // Date/Time Filters
        public bool? FilterByRecordingEndTime { get; set; }
        public TimeSpan? EarliestTimeOfDay { get; set; }
        public TimeSpan? LatestTimeOfDay { get; set; }

        // Duration Filters
        public int? MinimumDurationSeconds { get; set; }
        public int? MaximumDurationSeconds { get; set; }

        // Call Metadata Filters
        public string PhoneNumber { get; set; }
        public string CallDirection { get; set; } // e.g., "incoming", "outgoing", etc.
        public string DeviceNumber { get; set; }
        public string HuntGroup { get; set; }
        public string AccountCode { get; set; }
        public string CallId { get; set; }

        // Content Filters
        public string CommentContains { get; set; }
        public string TagName { get; set; }
        public string BookmarkText { get; set; }

        // Special Features Filters
        public bool? HasVideoRecording { get; set; }
        public bool? HasPciComplianceEvents { get; set; }
        public bool? HasQualityEvaluation { get; set; }

        // Recorder Filters
        public string RecorderType { get; set; } // e.g., "primary" or "backup"
        public string RecorderId { get; set; }

        // Pagination & Sorting
        public int? PageOffset { get; set; }
        public int? PageSize { get; set; }
        public string SortBy { get; set; } // e.g., "duration", "callId", etc.
        public bool? SortDescending { get; set; }

        // Advanced Filters
        public string RecordingGroupId { get; set; }
    }
}