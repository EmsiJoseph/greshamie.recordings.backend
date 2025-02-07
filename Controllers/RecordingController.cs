using System.Text.Json;
using backend.Classes;
using backend.Data;
using backend.DTOs;
using backend.Extensions;
using backend.Models;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.ClarifyGoServices.LiveRecordings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecordingsController : ControllerBase
{
    private readonly ILogger<RecordingsController> _logger;
    private readonly ILiveRecordingsService _liveRecordingsService;
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly ApplicationDbContext _context;

    public RecordingsController(
        ILogger<RecordingsController> logger,
        ILiveRecordingsService liveRecordingsService,
        IHistoricRecordingsService historicRecordingsService,
        ApplicationDbContext context
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _liveRecordingsService =
            liveRecordingsService ?? throw new ArgumentNullException(nameof(liveRecordingsService));
        _historicRecordingsService = historicRecordingsService ??
                                     throw new ArgumentNullException(nameof(historicRecordingsService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private async Task<List<RecordingDto>> SearchAndProcessRecordings(RecordingSearchFiltersDto? filtersDto)
    {
        var historicRecordingsSearchResult = await _historicRecordingsService.SearchRecordingsAsync(filtersDto);
        var historicRecordingSearchResults = historicRecordingsSearchResult.ToList();
        var historicRecordingsRaw = historicRecordingSearchResults.Select(x => x.HistoricRecording).ToList();

        return await Task.FromResult(historicRecordingsRaw.Select(x => new RecordingDto
        {
            Id = x.Id,
            Caller = x.CallingParty,
            Receiver = x.CalledParty,
            StartDateTime = x.MediaStartedTime,
            EndDateTime = x.MediaCompletedTime,
            CallType = x.CallType != null ? _context.CallTypes.Find(x.CallType)?.Name ?? string.Empty : string.Empty,
            IsLive = false, // TODO: Change this to dynamic if we know the shape of the live recordings
            DurationSeconds = x.MediaStartedTime != null && x.MediaCompletedTime != null
                ? (int)(x.MediaCompletedTime - x.MediaStartedTime).Value.TotalSeconds
                : 0,
            Recorder = x.RecorderId,
        }).ToList());
    }

    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("")]
    public async Task<IActionResult> GetRecordings()
    {
        var filtersDto = new RecordingSearchFiltersDto
        {
            StartDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday),
            EndDate = DateTime.Now
        };

        return Ok(await SearchAndProcessRecordings(filtersDto));
    }

    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("search")]
    public async Task<IActionResult> SearchRecordings([FromQuery] RecordingSearchFiltersDto filtersDto)
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

        return Ok(await SearchAndProcessRecordings(filtersDto));
    }
}