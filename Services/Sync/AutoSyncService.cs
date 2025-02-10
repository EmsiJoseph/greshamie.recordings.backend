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
using Microsoft.Extensions.DependencyInjection;

namespace backend.Services.Sync
{
    public class AutoSyncService : IAutoSyncService
    {
        private Timer _timer;
        private readonly ILogger<AutoSyncService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IHistoricRecordingsService _historicRecordingsService;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoSyncService(
            ILogger<AutoSyncService> logger,
            ITokenService tokenService,
            IHistoricRecordingsService historicRecordingsService,
            IConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _tokenService = tokenService;
            _historicRecordingsService = historicRecordingsService;
            _config = config;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auto Sync Service is starting.");
            _timer = new Timer(AutoSync, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void AutoSync(object state)
        {
            try
            {
                var adminUsername = _config["AdminCredentials:Username"];
                var adminPassword = _config["AdminCredentials:Password"];
                await _tokenService.SetBearerTokenWithPasswordAsync(adminUsername, adminPassword, _httpClient);

                var yesterday = DateTime.UtcNow.AddDays(-1).Date;
                var today = DateTime.UtcNow.Date;
                await SynchronizeRecordingsAsync(yesterday, today);

                _logger.LogInformation("Sync complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during synchronization.");
            }
        }

        public async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var blobStorageService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();

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
                    if (await dbContext.SyncedRecordings.AnyAsync(r => r.Id == Id))
                    {
                        _logger.LogInformation($"Skipping already synced recording: {Id}");
                        continue;
                    }

                    // Format the file name based on the recording date and ID.
                    var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

                    // Export the recording as an MP3 stream.
                    using var mp3Stream = await _historicRecordingsService.ExportMp3Async(Id);

                    // Upload the MP3 file to blob storage.
                    var blobUrl = await blobStorageService.UploadFileAsync(mp3Stream, "greshamrecordings", fileName);

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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auto Sync Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
