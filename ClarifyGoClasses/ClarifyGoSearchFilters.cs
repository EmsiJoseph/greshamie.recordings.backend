namespace backend.Classes;

public class ClarifyGoSearchFilters
{
    // Required parameters
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional parameters
    public bool? FilterRecordingByCompletionTime { get; set; }
    public TimeSpan? MinTimeOfDay { get; set; }
    public TimeSpan? MaxTimeOfDay { get; set; }
    public int? MinDurationSeconds { get; set; }
    public int? MaxDurationSeconds { get; set; }
    public string PhoneNumber { get; set; }
    public string Direction { get; set; }
    public string Device { get; set; }
    public string HuntGroupNumber { get; set; }
    public string AccountCode { get; set; }
    public string CallId { get; set; }
    public string CommentText { get; set; }
    public string Tag { get; set; }
    public string Bookmark { get; set; }
    public bool? SearchUnallocatedDevices { get; set; }
    public bool? HasScreenRecording { get; set; }
    public bool? HasPciEvents { get; set; }
    public bool? HasRecordingEvaluation { get; set; }
    public string RedundancyType { get; set; }
    public string RecorderFilter { get; set; }

    // Paging and sorting parameters
    public long FirstResultIndex { get; set; } = 0;
    public long MaxResults { get; set; } = 50;
    public string SortBy { get; set; }
    public bool SortDescending { get; set; } = false;

    // Additional parameters
    public Guid? RecordingGroupingId { get; set; }

    public bool Validate()
    {
        // Basic validation rules
        if (StartDate >= EndDate)
        {
            return false;
        }

        // Validate time of day range if either is specified
        if ((MinTimeOfDay.HasValue && !MaxTimeOfDay.HasValue) ||
            (!MinTimeOfDay.HasValue && MaxTimeOfDay.HasValue))
        {
            return false;
        }

        // Validate duration range
        if (MinDurationSeconds.HasValue && MaxDurationSeconds.HasValue &&
            MinDurationSeconds > MaxDurationSeconds)
        {
            return false;
        }

        // Validate direction enum if specified
        if (!string.IsNullOrEmpty(Direction))
        {
            var validDirections = new[] { "incoming", "outgoing", "internal", "external" };
            if (!Array.Exists(validDirections, d => d.Equals(Direction, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // Validate redundancy type if specified
        if (!string.IsNullOrEmpty(RedundancyType))
        {
            var validTypes = new[] { "primary", "backup" };
            if (!Array.Exists(validTypes, t => t.Equals(RedundancyType, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }
}