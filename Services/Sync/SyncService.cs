using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using backend.ClarifyGoClasses;
using backend.Data;
using backend.Data.Models;
using backend.DTOs;
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
    

    //Exports recordings matching the search filters to blob storage and saves new SyncedRecording records.
    private async Task<List<SyncedRecording>> ExportRecordingToBlob(RecordingSearchFiltersDto searchFilters)
    {
        var syncedRecordings = new List<SyncedRecording>();
        
        try
        {
            try
            {
                // Validate configuration
                string containerName = _config["BlobStorage:ContainerName"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    _logger.LogError("Missing or invalid BlobStorage:ContainerName configuration.");
                    throw new InvalidOperationException("Blob storage container name is missing or invalid.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate configuration for blob storage.");
                throw;
            }

            PagedResponseDto<HistoricRecordingSearchResult> pagedResponse;
            try
            {
                pagedResponse = await _historicRecordingsService.SearchProcessedRecordingsAsync(searchFilters);
                if (!pagedResponse.Items.Any())
                {
                    _logger.LogInformation("No recordings found for filters: StartDate={StartDate}, EndDate={EndDate}", 
                        searchFilters.StartDate, searchFilters.EndDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch recordings with filters: StartDate={StartDate}, EndDate={EndDate}", 
                    searchFilters.StartDate, searchFilters.EndDate);
                throw;
            }

            var results = pagedResponse.Items;
            foreach (var result in results)
            {
                var (id, mediaStartedTime) = (result.HistoricRecording.Id, result.HistoricRecording.MediaStartedTime);

                try
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        _logger.LogWarning("Skipping recording with missing Id.");
                        continue;
                    }

                    if (await _dbContext.SyncedRecordings.AnyAsync(r => r.Id == id))
                    {
                        _logger.LogInformation("Skipping already synced recording: {Id}", id);
                        continue;
                    }

                    if (mediaStartedTime == default)
                    {
                        _logger.LogWarning("Skipping recording with missing MediaStartedTime: {Id}", id);
                        continue;
                    }

                    var mp3FileName = $"{mediaStartedTime:yyyy/MM/dd}/{id}.mp3";
                    var jsonFileName = $"{mediaStartedTime:yyyy/MM/dd}/{id}.json";

                    string? downloadUrl;
                    string? streamingUrl;
                    try
                    {
                        await using var mp3Stream = await _historicRecordingsService.ExportMp3Async(id);
                        downloadUrl = await _blobStorageService.UploadFileAsync(mp3Stream, 
                            _config["BlobStorage:ContainerName"]!, mp3FileName);
                        streamingUrl = await _blobStorageService.StreamingUrlAsync(
                            _config["BlobStorage:ContainerName"]!, mp3FileName);
                        _logger.LogInformation("Uploaded MP3 for recording {Id} to {FileName}", id, mp3FileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to export or upload MP3 for recording {Id} to {FileName}", 
                            id, mp3FileName);
                        throw;
                    }

                    string? json;
                    try
                    {
                        var processedRecording = new ProcessedRawRecording
                        {
                            Recording = (ProcessedRecording)result.HistoricRecording, // Correct cast
                            ScreenRecordingCount = result.ScreenRecordingCount,
                            TagCount = result.TagCount,
                            CommentCount = result.CommentCount, // Fixed typo
                            PciEventCount = result.PciEventCount,
                            RecordingEvaluationCount = result.RecordingEvaluationCount
                        };

                        var jsonOptions = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };
                        json = JsonSerializer.Serialize(processedRecording, jsonOptions);
                        _logger.LogInformation("Serialized JSON metadata for recording {Id}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to serialize JSON metadata for recording {Id}", id);
                        throw;
                    }

                    try
                    {
                        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                        await _blobStorageService.UploadFileAsync(jsonStream, 
                            _config["BlobStorage:ContainerName"]!, jsonFileName);
                        _logger.LogInformation("Uploaded JSON metadata for recording {Id} to {FileName}", 
                            id, jsonFileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to upload JSON metadata for recording {Id} to {FileName}", 
                            id, jsonFileName);
                    }

                    var syncedRecording = new SyncedRecording
                    {
                        Id = id,
                        RecordingGroupID = result.HistoricRecording.RecordingGroupingId ?? string.Empty,
                        DownloadUrl = downloadUrl,
                        StreamingUrl = streamingUrl,
                        RecordingDate = mediaStartedTime,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        Caller = result.HistoricRecording.CallingParty ?? string.Empty,
                        Callee = result.HistoricRecording.CalledParty ?? string.Empty,
                        DurationSeconds = (int)(result.HistoricRecording.MediaCompletedTime - mediaStartedTime).TotalSeconds
                    };

                    try
                    {
                        _dbContext.SyncedRecordings.Add(syncedRecording);
                        await _dbContext.SaveChangesAsync();
                        syncedRecordings.Add(syncedRecording);
                        _logger.LogInformation("Saved SyncedRecording {Id} to database", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save SyncedRecording {Id} to database", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing recording {Id}", id);
                    throw;
                }
            }

            return syncedRecordings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting recordings to blob for search filters: StartDate={StartDate}, EndDate={EndDate}", 
                searchFilters.StartDate, searchFilters.EndDate);
            throw new Exception("Error exporting recordings to blob for search filters", ex);
        }
    }
    
    //Synchronizes recordings between the specified dates.
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
               _logger.LogInformation("No new recordings were synced for the session from {FromDate} to {ToDate}", 
                   fromDate, toDate);
           }

           // Serialize the synced recordings list to JSON.
           var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
           try
           {
               var json = JsonSerializer.Serialize(syncedRecordings, jsonOptions);
               _logger.LogInformation("Serialized sync session JSON for session from {FromDate} to {ToDate}", 
                   fromDate, toDate);
               try
               {
                   // Use the blob service to upload the JSON file.
                   var sessionJsonUrl = await _blobStorageService.UploadSyncSessionFileAsync(json);
                   _logger.LogInformation("Sync session JSON saved at {SessionJsonUrl}", sessionJsonUrl);
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Failed to upload sync session JSON for session from {FromDate} to {ToDate}", 
                       fromDate, toDate);
                   throw;
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to serialize synced recordings for session from {FromDate} to {ToDate}", 
                   fromDate, toDate);
               throw;
           }
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, "Error synchronizing recordings from {FromDate} to {ToDate}", 
               fromDate, toDate);
           throw new Exception($"Error synchronizing recordings from {fromDate} to {toDate}", ex);
       }
    }

  
    //Syncs a single missing recording using its recording ID.
    public async Task SyncRecordingByObjectAsync(RecordingDto dto)
    {
        try
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
            var syncedRecordings = await ExportRecordingToBlob(searchFilters);
            if (syncedRecordings.Count == 0)
            {
                _logger.LogInformation(
                    "No recordings were synced for RecordingDto: RecorderId={RecorderId}, StartDate={StartDate}, EndDate={EndDate}", 
                    dto.Recorder ?? "null", dto.StartDateTime, dto.EndDateTime);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully synced {Count} recording(s) for RecordingDto: RecorderId={RecorderId}", 
                    syncedRecordings.Count, dto.Recorder ?? "null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error syncing recording for RecordingDto: RecorderId={RecorderId}, StartDate={StartDate}, EndDate={EndDate}", 
                dto.Recorder ?? "null", dto.StartDateTime, dto.EndDateTime);
            throw;
        }
    }
}