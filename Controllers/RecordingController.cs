using System.Text.Json;
using backend.ClarifyGoClasses;
using backend.Constants;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Extensions;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.ClarifyGoServices.LiveRecordings;
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
    private readonly ILiveRecordingsService _liveRecordingsService;
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly ISyncService _syncService;
    private readonly ApplicationDbContext _context;

    public RecordingController(
        ILogger<RecordingController> logger,
        ILiveRecordingsService liveRecordingsService,
        IHistoricRecordingsService historicRecordingsService,
        ISyncService syncService,
        ApplicationDbContext context
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _liveRecordingsService =
            liveRecordingsService ?? throw new ArgumentNullException(nameof(liveRecordingsService));
        _historicRecordingsService = historicRecordingsService ??
                                     throw new ArgumentNullException(nameof(historicRecordingsService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
    }

    private async Task<List<RecordingDto>> MapRawRecordingToDto(
        List<ClarifyGoHistoricRecordingRaw> historicRecordingsRaw)
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
            DurationSeconds = x.MediaStartedTime != null && x.MediaCompletedTime != null
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
            var pagedResults = await _historicRecordingsService.SearchRecordingsAsync(filtersDto);

            var mappedItems = await MapRawRecordingToDto(pagedResults.Items.ToList());

            if (!string.IsNullOrEmpty(filtersDto?.Search))
            {
                mappedItems = mappedItems.Where(x =>
                    x.Caller.Contains(filtersDto.Search) ||
                    x.Receiver.Contains(filtersDto.Search)
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
            throw new ServiceException($"Unexpected error: {ex.Message}", 500);
        }
    }

    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("")]
    public async Task<IActionResult> SearchRecordings([FromQuery] RecordingSearchFiltersDto filtersDto)
    {
        try
        {
            if (filtersDto == null)
            {
                return BadRequest(new { message = "Search filters are required" });
            }

            // Ensure dates are set if not provided
            if (filtersDto.StartDate == default)
            {
                filtersDto.StartDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            }

            if (filtersDto.EndDate == default)
            {
                filtersDto.EndDate = DateTime.Now;
            }

            _logger.LogInformation("Search Filters: {Filters}", JsonSerializer.Serialize(filtersDto));


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
    public async Task<IActionResult> Synchronize([FromBody] SyncDateRangeDto request)
    {
        if (request == null)
        {
            return BadRequest(new { Message = "Invalid request body." });
        }

        try
        {
            await _syncService.SynchronizeRecordingsAsync(request.StartDate, request.EndDate);
            return Ok(new { Message = "Synchronization completed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronization failed.");
            return StatusCode(500, new { Message = "Error synchronizing recordings.", Error = ex.Message });
        }
    }
}