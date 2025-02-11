using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using backend.Services.Auth;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Services.Storage;

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

        public async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
        {
            // Create search filters and retrieve recordings data from Clarify Go.
            var searchFilters = new RecordingSearchFiltersDto
            {
                StartDate = fromDate,
                EndDate = toDate
            };

            var results = await _historicRecordingsService.SearchRecordingsAsync(searchFilters);

            // Process each recording in the returned list.
            foreach (var result in results)
            {
                var (Id, MediaStartedTime) = (result.HistoricRecording.Id, result.HistoricRecording.MediaStartedTime);
                // If the recording already exists, skip it.
                if (await _dbContext.SyncedRecordings.AnyAsync(r => r.Id == Id))
                {
                    _logger.LogInformation($"Skipping already synced recording: {Id}");
                    continue;
                }

                // Format the file name based on the recording date and ID.
                var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

                // Export the recording as an MP3 stream.
                using var mp3Stream = await _historicRecordingsService.ExportMp3Async(Id);

                // Get both URLs from blob storage
                var downloadUrl = await _blobStorageService.UploadFileAsync(mp3Stream, "greshamrecordings", fileName);
                
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

                _dbContext.SyncedRecordings.Add(syncedRecording);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}