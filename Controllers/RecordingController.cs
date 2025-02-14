using System.Security.Claims;
using Asp.Versioning;
using backend.ClarifyGoClasses;
using backend.Constants;
using backend.Constants.Audit;
using backend.Data;
using backend.Data.Models;
using backend.DTOs;
using backend.DTOs.Recording;
using backend.Exceptions;
using backend.Extensions;
using backend.Services.Audits;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.Sync;
using backend.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace backend.Controllers;

[Authorize]
[ApiController]
[ApiVersion(ApiVersionConstants.VersionString)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RecordingController(
    ILogger<RecordingController> logger,
    IHistoricRecordingsService historicRecordingsService,
    ISyncService syncService,
    ApplicationDbContext context,
    IAuditService auditService,
    IBlobStorageService blobStorageService)
    : ControllerBase
{
    private readonly ILogger<RecordingController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IHistoricRecordingsService _historicRecordingsService = historicRecordingsService ??
                                                                             throw new ArgumentNullException(
                                                                                 nameof(historicRecordingsService));

    private readonly ISyncService _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private readonly IAuditService
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));

    private readonly IBlobStorageService _blobStorageService = blobStorageService ??
                                                               throw new ArgumentNullException(
                                                                   nameof(blobStorageService));

    private async Task<List<RecordingDto>> MapRawRecordingToDto(
        List<HistoricRecordingRaw> historicRecordingsRaw)
    {
        return await Task.FromResult(historicRecordingsRaw.Select(x => new RecordingDto
        {
            Id = x.Id,
            Caller = x.CallingParty,
            Receiver = x.CalledParty,
            StartDateTime = x.MediaStartedTime,
            EndDateTime = x.MediaCompletedTime,
            CallType = x.CallType != null
                ? _context.CallTypes.FirstOrDefault(ct => ct.IdFromClarify == x.CallType)?.NormalizedName ??
                  string.Empty
                : string.Empty,
            IsLive = false, // TODO: Change this to dynamic if we know the shape of the live recordings
            DurationSeconds =
                (int)(x.MediaCompletedTime - x.MediaStartedTime).TotalSeconds,
            Recorder = x.RecorderId,
        }).ToList());
    }

    private async Task<PagedResponseDto<RecordingDto>> SearchAndProcessRecordings(
        RecordingSearchFiltersDto? filtersDto)
    {
        try
        {
            var pagedResults =
                await _historicRecordingsService.SearchRecordingsAsync(filtersDto ?? new RecordingSearchFiltersDto());

            var mappedItems = await MapRawRecordingToDto(pagedResults.Items.ToList());


            return new PagedResponseDto<RecordingDto>
            {
                Items = mappedItems,
                PageOffSet = pagedResults.PageOffSet,
                PageSize = pagedResults.PageSize,
                TotalPages = pagedResults.TotalPages,
                TotalCount = pagedResults.TotalCount,
                HasNext = pagedResults.HasNext,
                HasPrevious = pagedResults.HasPrevious
            };
        }
        catch (ServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while searching recordings");
            throw new ServiceException($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a synced recording from the database. If it doesn't exist,
    /// it calls the sync service to export and sync the recording.
    /// </summary>
    private async Task<SyncedRecording> GetSyncedRecordingAsync(RecordingDto dto)
    {
        // Attempt to get the synced recording from the database.
        var syncedRecording = await _context.SyncedRecordings.FindAsync(dto.Id);

        if (syncedRecording == null)
        {
            // If not found, attempt to sync the recording.
            await _syncService.SyncRecordingByObjectAsync(dto);
            syncedRecording = await _context.SyncedRecordings.FindAsync(dto.Id);
        }
        else
        {
            return syncedRecording;
        }

        return syncedRecording ?? throw new Exception("Synced recording not found.");
    }

    /// <summary>
    /// Search for recordings with various filters.
    /// Results are cached for 60 seconds to improve performance.
    /// </summary>
    [OutputCache(Duration = 60, VaryByQueryKeys = ["RecordingsCache"])]
    [HttpGet("")]
    public async Task<IActionResult> SearchRecordings([FromQuery] RecordingSearchFiltersDto? filtersDto)
    {
        try
        {
            if (filtersDto == null)
            {
                return BadRequest(new { message = "Search filters are required" });
            }

            var results = await SearchAndProcessRecordings(filtersDto);
            return Ok(results);
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SearchRecordings");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Sync recordings from a specific date range.
    /// This will fetch recordings from ClarifyGo and store them locally.
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Synchronize([FromBody] SyncDateRangeDto? request)
    {
        if (request == null)
        {
            return BadRequest(new { Message = "Invalid request body." });
        }

        try
        {
            await _syncService.SynchronizeRecordingsAsync(request.StartDate, request.EndDate);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Log the recording sync event using your audit service with date range
            string logMessage =
                $"User synced recordings from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}";
            var auditEntry = new AuditEntry
            {
                UserId = userId,
                EventId = AuditEventTypes.ManualSync,
                Details = logMessage
            };

            await _auditService.LogAuditEntryAsync(auditEntry);


            return Ok(new { Message = "Synchronization completed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronization failed.");
            return StatusCode(500, new { Message = "Error synchronizing recordings.", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get a URL to stream a recording.
    /// The URL is temporary and will expire after a short time.
    /// </summary>
    [HttpGet("stream")]
    public async Task<IActionResult> GetStreamingUrl([FromQuery] RecordingDto dto)
    {
        try
        {
            var syncedRecording = await GetSyncedRecordingAsync(dto);
            if (syncedRecording != null)
            {
                var updateSaSUrl = await _blobStorageService.UpdateStreamingUrlAsync(dto.Id);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            string logMessage = $"User played the recording.";

            var auditEntry = new AuditEntry
            {
                UserId = userId,
                EventId = AuditEventTypes.RecordPlayed,
                RecordId = syncedRecording.Id,
                Details = logMessage
            };

            await _auditService.LogAuditEntryAsync(auditEntry);

            return Ok(new { syncedRecording.StreamingUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving streaming URL", Error = ex.Message });
        }
    }

    /// <summary>
    /// Get a URL to download a recording.
    /// The URL is temporary and will expire after a short time.
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> GetDownloadUrl([FromQuery] RecordingDto dto)
    {
        try
        {
            var syncedRecording = await GetSyncedRecordingAsync(dto);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            string logMessage = $"User downloaded the recording.";

            var auditEntry = new AuditEntry
            {
                UserId = userId,
                EventId = AuditEventTypes.RecordExported,
                RecordId = syncedRecording.Id,
                Details = logMessage
            };

            await _auditService.LogAuditEntryAsync(auditEntry);

            return Ok(new { syncedRecording.DownloadUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving download URL", Error = ex.Message });
        }
    }
}