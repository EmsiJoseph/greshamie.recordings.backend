using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

public class RecordingSyncService
{
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ApplicationDbContext _dbContext;

    public RecordingSyncService(
        IHistoricRecordingsService historicRecordingsService,
        IBlobStorageService blobStorageService,
        ApplicationDbContext dbContext)
    {
        _historicRecordingsService = historicRecordingsService;
        _blobStorageService = blobStorageService;
        _dbContext = dbContext;
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
                Console.WriteLine($"Skipping already synced recording: {Id}");
                continue;
            }
            
            // Format the file name based on the recording date and ID.
            var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

            // Export the recording as an MP3 stream.
            using var mp3Stream = await _historicRecordingsService.ExportMp3Async(Id);

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

            _dbContext.SyncedRecordings.Add(syncedRecording);
            await _dbContext.SaveChangesAsync();
        }
    }

    // Formats the folder path using the recording's date (e.g. recordings/2025/01/31)
    private string ProcessFolderPath(DateTime recordingDate)
    {
        return $"recordings/{recordingDate:yyyy/MM/dd}";
    }
}

[ApiController]
[Route("api/[controller]")]
public class SyncRecordingController : ControllerBase
{
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ApplicationDbContext _dbContext;

    public SyncRecordingController(
        IHistoricRecordingsService historicRecordingsService,
        IBlobStorageService blobStorageService,
        ApplicationDbContext dbContext)
    {
        _historicRecordingsService = historicRecordingsService;
        _blobStorageService = blobStorageService;
        _dbContext = dbContext;
    }

    [HttpPost("synchronize")]
    public async Task<IActionResult> SynchronizeRecordings([FromBody] SyncDateRangeDto dateRange)
    {
        try
        {
            if (dateRange == null || dateRange.StartDate == default || dateRange.EndDate == default)
            {
                return BadRequest(new { message = "Start date and end date are required" });
            }

            await SynchronizeRecordingsAsync(dateRange.StartDate, dateRange.EndDate);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
        }
    }

    private async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
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
                Console.WriteLine($"Skipping already synced recording: {Id}");
                continue;
            }
            
            // Format the file name based on the recording date and ID.
            var fileName = $"{MediaStartedTime:yyyy/MM/dd}/{Id}.mp3";

            // Export the recording as an MP3 stream.
            using var mp3Stream = await _historicRecordingsService.ExportMp3Async(Id);

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

            _dbContext.SyncedRecordings.Add(syncedRecording);
            await _dbContext.SaveChangesAsync();
        }
    }
}
