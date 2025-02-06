using System;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class RecordingSearchFiltersDto
    {
        // Start date and time (inclusive) of the search period in UTC.
        // Example: "2022-01-15T09:00:00Z"
        public DateTime? StartDate { get; set; }

        // End date and time (inclusive) of the search period in UTC.
        // Example: "2022-01-25T18:00:00Z"
        public DateTime? EndDate { get; set; }

        // Specify whether to filter recordings by completion time (true) or start time (false).
        // Default is false if not provided.
        public bool? FilterByRecordingEndTime { get; set; } = false;

        // Only include recordings that occurred on or after this time of day.
        // Expected as a TimeSpan (e.g., 09:00:00).
        public TimeSpan? EarliestTimeOfDay { get; set; }

        // Only include recordings that occurred on or before this time of day.
        // Expected as a TimeSpan (e.g., 18:00:00).
        public TimeSpan? LatestTimeOfDay { get; set; }

        // Only include recordings with a duration greater than or equal to this value (in seconds).
        [Range(0, int.MaxValue)]
        public int? MinimumDurationSeconds { get; set; }

        // Only include recordings with a duration less than or equal to this value (in seconds).
        [Range(0, int.MaxValue)]
        public int? MaximumDurationSeconds { get; set; }

        // Only include recordings where the calling or called party contains these digits.
        // Example: "+441202"
        public string? PhoneNumber { get; set; }

        // Only include recordings where the call direction matches this value.
        // Possible values: "incoming", "outgoing", "internal", "external".
        public string? CallDirection { get; set; }

        // Only include recordings involving the specified device number.
        // Can be a single value, a range, a comma-separated list, or a wildcarded string.
        public string? DeviceNumber { get; set; }

        // Only include recordings where the call was routed through the specified hunt group number.
        // Example: "500"
        public string? HuntGroupNumber { get; set; }

        // Only include recordings associated with the specified account code.
        public string? AccountCode { get; set; }

        // Only include recordings associated with the specified call id.
        public string? CallId { get; set; }

        // Only include recordings with comments containing the specified text.
        public string? CommentContains { get; set; }

        // Only include recordings tagged with the specified tag name.
        public string? TagName { get; set; }

        // Only include recordings with bookmarks containing the specified text.
        public string? BookmarkText { get; set; }

        // Not in use; possible values: true or false.
        public bool? SearchUnallocatedDevices { get; set; } = false;

        // Only include recordings that either do or do not have an associated video.
        public bool? HasScreenRecording { get; set; }

        // Only include recordings that either do or do not have PCI events.
        public bool? HasPciComplianceEvents { get; set; }

        // Only include recordings that either do or do not have an associated evaluation.
        public bool? HasQualityEvaluation { get; set; }

        // Only include recordings made by a primary or backup recorder.
        // Possible values: "primary" or "backup".
        public string? RecorderType { get; set; }

        // Only include recordings made by the specified recorder.
        public string? RecorderId { get; set; }

        // Zero-based index of the first result to return (for paging).
        // Default is 0.
        public long? PageOffset { get; set; } = 0;

        // Maximum number of results to return in this page.
        // Default is 50.
        public long? PageSize { get; set; } = 50;

        // Specifies how to sort the results.
        // Possible values: "type", "from", "to", "duration", "isprimary".
        public string? SortBy { get; set; }

        // Whether to sort the results in descending order.
        // Default is false (ascending order).
        public bool? SortDescending { get; set; } = false;

        // Only include recordings that are grouped together using the specified ID.
        // Example: "6e348525-86f6-4d6b-8f78-1e28f28f1e49"
        public string? RecordingGroupId { get; set; }
    }
}
