using System.Security.Claims;
using System.Text.Json;
using Azure.Core;
using backend.ClarifyGoClasses;
using backend.Constants;
using backend.Data;
using backend.Data.Models;
using backend.DTOs;
using backend.DTOs.Recording;
using backend.Exceptions;
using backend.Extensions;
using backend.Services.Audits;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace backend.Controllers;

[Authorize]
[ApiController]
[ApiVersion(ApiVersionConstants.VersionString)]
[Route("api/v{version:apiVersion}/[controller]")]
public class RecordingController : ControllerBase
{
    private readonly ILogger<RecordingController> _logger;
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly ISyncService _syncService;
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;


    public RecordingController(
        ILogger<RecordingController> logger,
        IHistoricRecordingsService historicRecordingsService,
        ISyncService syncService,
        ApplicationDbContext context,
        IAuditService auditService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _liveRecordingsService =
            liveRecordingsService ?? throw new ArgumentNullException(nameof(liveRecordingsService));
        _historicRecordingsService = historicRecordingsService ??
                                     throw new ArgumentNullException(nameof(historicRecordingsService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

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
            DurationSeconds = x is { MediaStartedTime: not null, MediaCompletedTime: not null }
                ? (int)(x.MediaCompletedTime - x.MediaStartedTime).Value.TotalSeconds
                : 0,
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

            if (!string.IsNullOrEmpty(filtersDto?.Search))
            {
                mappedItems = mappedItems.Where(x =>
                    x is { Caller: not null, Receiver: not null } && (x.Caller.Contains(filtersDto.Search) ||
                                                                      x.Receiver.Contains(filtersDto.Search))
                ).ToList();
            }

            return new PagedResponseDto<RecordingDto>
            {
                Items = mappedItems,
                PageOffset = pagedResults.PageOffset,
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

        return syncedRecording ?? throw new Exception("Synced recording not found.");
    }

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

            // Ensure dates are set if not provided
            filtersDto.StartDate ??= DateTime.Now.StartOfWeek(DayOfWeek.Monday);

            filtersDto.EndDate ??= DateTime.Now;

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
            string logMessage = $"User synced recordings from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}";
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.ManualSync, null, logMessage);


            return Ok(new { Message = "Synchronization completed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronization failed.");
            return StatusCode(500, new { Message = "Error synchronizing recordings.", Error = ex.Message });
        }
    }

    [HttpGet("stream")]
    public async Task<IActionResult> GetStreamingUrl([FromQuery] RecordingDto dto)
    {
        try
        {
            var syncedRecording = await GetSyncedRecordingAsync(dto);
            if (syncedRecording == null)
            {
                return NotFound(new { Message = "Recording not found." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            string logMessage = $"User played the recording.";
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.RecordPlayed, null, logMessage);

            return Ok(new { syncedRecording.StreamingUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving streaming URL", Error = ex.Message });
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> GetDownloadUrl([FromQuery] RecordingDto dto)
    {
        try
        {
            var syncedRecording = await GetSyncedRecordingAsync(dto);
            if (syncedRecording == null)
            {
                return NotFound(new { Message = "Recording not found." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            string logMessage = $"User downloaded the recording.";
            await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.RecordExported, null, logMessage);

            return Ok(new { syncedRecording.DownloadUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Error retrieving download URL", Error = ex.Message });
        }
    }
}