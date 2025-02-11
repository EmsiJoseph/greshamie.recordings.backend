using System.Text.Json;
using backend.Classes;
using backend.Constants;
using backend.Data;
using backend.DTOs;
using backend.Exceptions;
using backend.Extensions;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.ClarifyGoServices.LiveRecordings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecordingController : ControllerBase
{
    private readonly ILogger<RecordingController> _logger;
    private readonly ILiveRecordingsService _liveRecordingsService;
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly ApplicationDbContext _context;

    public RecordingController(
        ILogger<RecordingController> logger,
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
        try
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
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Error searching recordings");
            throw;
        }
        catch (Exception ex)
        {
            // Something unexpected happened
            _logger.LogError(ex, "Unexpected error while searching recordings");
            throw new ServiceException($"Unexpected error: {ex.Message}", 500);
        }
    }

    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("")]
    public async Task<IActionResult> GetRecordings()
    {
        try
        {
            var filtersDto = new RecordingSearchFiltersDto
            {
                StartDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday),
                EndDate = DateTime.Now
            };

            var results = await SearchAndProcessRecordings(filtersDto);
            return Ok(results);
        }
        catch (ServiceException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetRecordings");
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "RecordingsCache" })]
    [HttpGet("search")]
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
    
    [Authorize(Roles = $"{RolesConstants.Admin}")]
    [HttpDelete("{recordingId}")]
    public async Task<IActionResult> DeleteRecording(string recordingId)
    {
        try
        {
            if (string.IsNullOrEmpty(recordingId))
            {
                return BadRequest(new { message = "Recording ID is required" });
            }

            bool results = await _historicRecordingsService.DeleteRecordingAsync(recordingId);
            if (!results)
            {
                return NotFound(new { message = "Recording not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting recording {RecordingId}", recordingId);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
    
    [Authorize(Roles = $"{RolesConstants.Admin}")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok("Admin only");
    }


    [HttpGet("{recordingId}/mp3")]
    public async Task<IActionResult> GetRecording(string recordingId)
    {
        try
        {
            if (string.IsNullOrEmpty(recordingId))
            {
                return BadRequest(new { message = "Recording ID is required" });
            }

            var recording = await _historicRecordingsService.ExportMp3Async(recordingId);
            if (recording == null)
            {
                return NotFound(new { message = "Recording not found" });
            }

            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting recording {RecordingId}", recordingId);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }

    [HttpGet("{recordingId}/wav")]
    public async Task<IActionResult> GetRecordingWav(string recordingId)
    {
        try
        {
            if (string.IsNullOrEmpty(recordingId))
            {
                return BadRequest(new { message = "Recording ID is required" });
            }

            var recording = await _historicRecordingsService.ExportWavAsync(recordingId);
            if (recording == null)
            {
                return NotFound(new { message = "Recording not found" });
            }

            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting recording {RecordingId}", recordingId);
            return StatusCode(500, new { message = "An unexpected error occurred" });
        }
    }
}