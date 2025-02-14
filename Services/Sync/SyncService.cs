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
    /// <summary>
    /// Exports recordings matching the search filters to blob storage and saves new SyncedRecording records.
    /// Throws an exception if an error occurs.
    /// </summary>
    private async Task<bool> ExportRecordingToBlob(RecordingSearchFiltersDto searchFilters)
    {
        try
        {
            var pagedResponse = await _historicRecordingsService.SearchRecordingsAsync(searchFilters);
            var results = pagedResponse.Items;
            foreach (var result in results)
            {
                var (id, mediaStartedTime) =
                    (result.Id, result.MediaStartedTime);

                // If the recording already exists, skip it.
                if (await _dbContext.SyncedRecordings.AnyAsync(r => r.Id == id))
                {
                    _logger.LogInformation($"Skipping already synced recording: {id}");
                    continue;
                }

                if (mediaStartedTime == null)
                {
                    _logger.LogWarning($"Skipping recording with missing MediaStartedTime: {id}");
                    continue;
                }

                // Format the file name based on the recording date and ID.
                var fileName = $"{mediaStartedTime:yyyy/MM/dd}/{id}.mp3";

                // Export the recording as an MP3 stream.
                await using var mp3Stream = await _historicRecordingsService.ExportMp3Async(id ?? string.Empty);

                // Upload the file and get both URLs from blob storage.
                var downloadUrl =
                    await _blobStorageService.UploadFileAsync(mp3Stream, _config["BlobStorage:ContainerName"], fileName);
                var streamingUrl = await _blobStorageService.StreamingUrlAsync(_config["BlobStorage:ContainerName"], fileName);

                // Save the new record to the SyncedRecordings table.
                var syncedRecording = new SyncedRecording
                {
                    Id = id ?? string.Empty,
                    DownloadUrl = downloadUrl,
                    StreamingUrl = streamingUrl,
                    RecordingDate = mediaStartedTime,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _dbContext.SyncedRecordings.Add(syncedRecording);
                await _dbContext.SaveChangesAsync();
                
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recordings to blob for search filters: {@SearchFilters}",
                searchFilters);
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
            // Create search filters to retrieve recordings from Clarify Go.
            var searchFilters = new RecordingSearchFiltersDto
            {
                StartDate = fromDate,
                EndDate = toDate
            };

            // Export the recordings and sync them to blob storage.
            bool exported = await ExportRecordingToBlob(searchFilters);
            if (!exported)
            {
                throw new Exception("Export failed for the specified recordings.");
            }
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