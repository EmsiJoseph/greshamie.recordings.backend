using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using backend.Data;
using backend.Data.Models;
using backend.DTOs.Recording;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Sync;

public class SyncService(
    IHistoricRecordingsService historicRecordingsService,
    ApplicationDbContext dbContext,
    IBlobStorageService blobStorageService,
    ILogger<SyncService> logger,
    IConfiguration config)
    : ISyncService
{
    private readonly ILogger<SyncService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IHistoricRecordingsService _historicRecordingsService =
        historicRecordingsService ?? throw new ArgumentNullException(nameof(historicRecordingsService));

    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly IBlobStorageService _blobStorageService =
        blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    
    // <summary>
    /// Exports recordings matching the search filters to blob storage and saves new SyncedRecording records.
    /// Throws an exception if an error occurs.
    /// </summary>
    private async Task<List<SyncedRecording>> ExportRecordingToBlob(RecordingSearchFiltersDto searchFilters)
    {
        var syncedRecordings = new List<SyncedRecording>();

        try
        {
            var pagedResponse = await _historicRecordingsService.SearchProcessedRecordingsAsync(searchFilters);
            var results = pagedResponse.Items;
            foreach (var result in results)
            {
                var (id, mediaStartedTime) = (result.HistoricRecording.Id, result.HistoricRecording.MediaStartedTime);

                if (await _dbContext.SyncedRecordings.AnyAsync(r => r.Id == id))
                {
                    _logger.LogInformation("Skipping already synced recording: {Id}", id);
                    continue;
                }

                if (mediaStartedTime == null)
                {
                    _logger.LogWarning("Skipping recording with missing MediaStartedTime: {Id}", id);
                    continue;
                }

                var mp3FileName = $"{mediaStartedTime:yyyy/MM/dd}/{id}.mp3";
                var jsonFileName = $"{mediaStartedTime:yyyy/MM/dd}/{id}.json";

                await using var mp3Stream = await _historicRecordingsService.ExportMp3Async(id ?? string.Empty);
                var downloadUrl = await _blobStorageService.UploadFileAsync(mp3Stream, _config["BlobStorage:ContainerName"], mp3FileName);
                var streamingUrl = await _blobStorageService.StreamingUrlAsync(_config["BlobStorage:ContainerName"], mp3FileName);

                // Map to ProcessedRawRecording
                var processedRecording = new ProcessedRawRecording
                {
                    Recording = (ProcessedRecording)result.HistoricRecording, // Already processed
                    ScreenRecordingCount = result.ScreenRecordingCount,
                    TagCount = result.TagCount,
                    CommentCount = result.CommentCount,
                    PciEventCount = result.PciEventCount,
                    RecordingEvaluationCount = result.RecordingEvaluationCount
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(processedRecording, jsonOptions);

                try
                {
                    using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                    await _blobStorageService.UploadFileAsync(jsonStream, _config["BlobStorage:ContainerName"], jsonFileName);
                    _logger.LogInformation("Uploaded JSON metadata for recording {Id} to {FileName}", id, jsonFileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to upload JSON metadata for recording {Id} to {FileName}", id, jsonFileName);
                }

                var syncedRecording = new SyncedRecording
                {
                    Id = id ?? string.Empty,
                    RecordingGroupID = result.HistoricRecording.RecordingGroupingId,
                    DownloadUrl = downloadUrl,
                    StreamingUrl = streamingUrl,
                    RecordingDate = mediaStartedTime,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    Caller = result.HistoricRecording.CallingParty,
                    Callee = result.HistoricRecording.CalledParty,
                    DurationSeconds = (int)(result.HistoricRecording.MediaCompletedTime - mediaStartedTime).TotalSeconds
                };

                _dbContext.SyncedRecordings.Add(syncedRecording);
                await _dbContext.SaveChangesAsync();
                syncedRecordings.Add(syncedRecording);
            }

            return syncedRecordings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recordings to blob for search filters: StartDate={StartDate}, EndDate={EndDate}", searchFilters.StartDate, searchFilters.EndDate);
            throw new Exception("Error exporting recordings to blob for search filters", ex);
        }
    }

    /// <summary>
    /// Synchronizes recordings between the specified dates.
    /// Throws an exception if the synchronization fails.
    /// </summary>
    public async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            // Create search filters to retrieve recordings.
            var searchFilters = new RecordingSearchFiltersDto
            {
                StartDate = fromDate,
                EndDate = toDate,
                PageOffset = 0,
                PageSize = 1000000 // Retrieve all recordings in one go
            };

            // Export the recordings and obtain the list of newly synced recordings.
            var syncedRecordings = await ExportRecordingToBlob(searchFilters);
            if (syncedRecordings.Count == 0)
            {
                _logger.LogInformation("No new recordings were synced for the session from {FromDate} to {ToDate}", fromDate, toDate);
            }

            // Serialize the synced recordings list to JSON.
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            var json = JsonSerializer.Serialize(syncedRecordings, jsonOptions);

            // Use the blob service to upload the JSON file.
            var sessionJsonUrl = await _blobStorageService.UploadSyncSessionFileAsync(json);
            _logger.LogInformation("Sync session JSON saved at {SessionJsonUrl}", sessionJsonUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing recordings from {FromDate} to {ToDate}", fromDate, toDate);
            throw new Exception($"Error synchronizing recordings from {fromDate} to {toDate}", ex);
        }
    }

    /// <summary>
    /// Syncs a single missing recording using its recording ID.
    /// Throws an exception if the operation fails.
    /// </summary>
    public async Task SyncRecordingByObjectAsync(RecordingDto dto)
    {
        var searchFilters = new RecordingSearchFiltersDto
        {
            StartDate = dto.StartDateTime,
            EndDate = dto.EndDateTime,
            CallDirection = dto.CallType,
            MinimumDurationSeconds = dto.DurationSeconds,
            MaximumDurationSeconds = dto.DurationSeconds,
            RecorderId = dto.Recorder
        };
        await ExportRecordingToBlob(searchFilters);
    }
}