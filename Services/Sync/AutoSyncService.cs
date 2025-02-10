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
    public class AutoSyncService(
        ILogger<AutoSyncService> logger,
        ITokenService tokenService,
        IHistoricRecordingsService historicRecordingsService,
        IConfiguration config,
        ApplicationDbContext dbContext,
        HttpClient httpClient,
        IBlobStorageService blobStorageService)
        : IAutoSyncService
    {
        private Timer _timer;
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly ITokenService _tokenService =
            tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly ILogger<AutoSyncService> logger;
        private readonly IHistoricRecordingsService historicRecordingsService;
        private readonly IConfiguration config;
        private readonly ApplicationDbContext dbContext;
        private readonly IBlobStorageService _blobStorageService;
        
        public Task StartAsync(CancellationToken cancellationToken)
        
        {
            logger.LogInformation("Auto Sync Service is starting.");
            _timer = new Timer(AutoSync, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void AutoSync(object state)
        {
            try
            {
                var adminUsername = config["AdminCredentials:Username"];
                var adminPassword = config["AdminCredentials:Password"];
                await _tokenService.SetBearerTokenWithPasswordAsync(adminUsername, adminPassword, _httpClient);
                

                var yesterday = DateTime.UtcNow.AddDays(-1).Date;
                var today = DateTime.UtcNow.Date;
                await SynchronizeRecordingsAsync(yesterday, today);

                logger.LogInformation("Sync complete");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during synchronization.");
            }
        }

        public async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
        {
            // Create search filters and retrieve recordings data from Clarify Go.
            var searchFilters = new RecordingSearchFiltersDto
            {
                StartDate = fromDate,
                EndDate = toDate
            };

            var results = await historicRecordingsService.SearchRecordingsAsync(searchFilters);

            // Process each recording in the returned list.
            foreach (var result in results)
            {
                var (Id, MediaStartedTime) = (result.HistoricRecording.Id, result.HistoricRecording.MediaStartedTime);
                // If the recording already exists, skip it.
                if (await dbContext.SyncedRecordings.AnyAsync(r => r.Id == Id))
                {
                    logger.LogInformation($"Skipping already synced recording: {Id}");
                    continue;
                }

                // Format the file name based on the recording date and ID.
                var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

                // Export the recording as an MP3 stream.
                using var mp3Stream = await historicRecordingsService.ExportMp3Async(Id);

                // Upload the MP3 file to blob storage.
                var blobUrl = await _blobStorageService.UploadFileAsync(mp3Stream, "greshamrecordings", fileName);

                // Save the new record to the SyncedRecordings table.
                var syncedRecording = new SyncedRecording
                {
                    Id = Id,
                    BlobUrl = blobUrl,
                    RecordingDate = MediaStartedTime?.Date ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.SyncedRecordings.Add(syncedRecording);
                await dbContext.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Auto Sync Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
