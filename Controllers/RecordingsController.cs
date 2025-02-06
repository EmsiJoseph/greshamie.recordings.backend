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
public class RecordingsController(
    ILogger<RecordingsController> logger,
    ILiveRecordingsService liveRecordingsService,
    IHistoricRecordingsService historicRecordingsService,
    ApplicationDbContext context
) : ControllerBase
{
    private readonly ILogger<RecordingsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ILiveRecordingsService _liveRecordingsService =
        liveRecordingsService ?? throw new ArgumentNullException(nameof(liveRecordingsService));

    private readonly IHistoricRecordingsService _historicRecordingsService =
        historicRecordingsService ?? throw new ArgumentNullException(nameof(historicRecordingsService));

    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));


    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("")]
    public async Task<IActionResult> GetRecordings([FromBody] RecordingSearchFiltersDto filtersDto)
    {
        filtersDto.StartDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
        filtersDto.EndDate = DateTime.Now;

        // var liveRecordings = await _liveRecordingsService.GetLiveRecordingsAsync();

        var historicRecordingsSearchResult = await _historicRecordingsService.SearchRecordingsAsync(filtersDto);

        var historicRecordingSearchResults = historicRecordingsSearchResult.ToList();
        var historicRecordingsRaw = historicRecordingSearchResults.Select(x => x.HistoricRecording).ToList();

        var recordingsProcessed = historicRecordingsRaw.Select(x => new Recording
        {
            Id = x.Id,
            Caller = x.CallingParty,
            Receiver = x.CalledParty,
            FromDateTime = x.MediaStartedTime,
            ToDateTime = x.MediaCompletedTime,
            // CallTypeId = x.CallType != null ? _context.CallTypes.Find(x.CallType)?.Name : null,
            IsLive = false,
            DurationSeconds = x.MediaStartedTime != null && x.MediaCompletedTime != null
                ? (int)(x.MediaCompletedTime - x.MediaStartedTime).Value.TotalSeconds
                : 0,
            Recorder = x.RecorderId,
        }).ToList();

        return Ok(recordingsProcessed);
    }
}