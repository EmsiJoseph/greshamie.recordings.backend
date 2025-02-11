using System;
using System.Threading.Tasks;
using System.Threading.Tasks;
using backend.Services.Auth;
using backend.Models;
using backend.Services.Auth;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Services.Sync
{
    public class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IHistoricRecordingsService _historicRecordingsService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IBlobStorageService _blobStorageService;
        private readonly HttpClient _httpClient;

        public SyncService(
            ILogger<SyncService> logger,
            ITokenService tokenService,
            IHistoricRecordingsService historicRecordingsService,
            ApplicationDbContext dbContext,
            IBlobStorageService blobStorageService,
            HttpClient httpClient)
        {
            _logger = logger;
            _tokenService = tokenService;
            _historicRecordingsService = historicRecordingsService;
            _dbContext = dbContext;
            _blobStorageService = blobStorageService;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Exports recordings matching the search filters to blob storage and saves new SyncedRecording records.
        /// Throws an exception if an error occurs.
        /// </summary>
        private async Task<bool> ExportRecordingToBlob(RecordingSearchFiltersDto searchFilters)
        {
            try
            {
                var results = await _historicRecordingsService.SearchRecordingsAsync(searchFilters);
                foreach (var result in results)
                {
                    var (Id, MediaStartedTime) =
                        (result.HistoricRecording.Id, result.HistoricRecording.MediaStartedTime);

                    // If the recording already exists, skip it.
                    if (await _dbContext.SyncedRecordings.AnyAsync(r => r.Id == Id))
                    {
                        _logger.LogInformation($"Skipping already synced recording: {Id}");
                        continue;
                    }

                    if (MediaStartedTime == null)
                    {
                        _logger.LogWarning($"Skipping recording with missing MediaStartedTime: {Id}");
                        continue;
                    }

                    // Format the file name based on the recording date and ID.
                    var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

                    // Export the recording as an MP3 stream.
                    // Upload the file and get both URLs from blob storage.
                    var downloadUrl =
                        await _blobStorageService.UploadFileAsync(mp3Stream, "greshamrecordings", fileName);
                    var streamingUrl = await _blobStorageService.StreamingUrlAsync("greshamrecordings", fileName);
                
                    // Save the new record to the SyncedRecordings table.
                    var syncedRecording = new SyncedRecording
                    {
                        Id = Id,
                        DownloadUrl = downloadUrl,
                        StreamingUrl = streamingUrl,
                        RecordingDate = MediaStartedTime?.Date ?? DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    IsDeleted = false
                };

                    _dbContext.SyncedRecordings.Add(syncedRecording);
                    await _dbContext.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting recordings to blob for search filters: {@SearchFilters}", searchFilters);
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
        public async Task SyncRecordingByIdAsync(string recordingId)
        {
            var searchFilters = new RecordingSearchFiltersDto
            {
                CallId = recordingId
            };
            await ExportRecordingToBlob(searchFilters);
        }
    }
}
