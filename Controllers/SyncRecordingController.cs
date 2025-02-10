using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services.ClarifyGoServices.HistoricRecordings;
using backend.Services.Storage;
using backend.Services.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SyncRecordingController : ControllerBase
{
    private readonly IHistoricRecordingsService _historicRecordingsService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IAutoSyncService _autoSyncService;
    private readonly ApplicationDbContext _dbContext;

    public SyncRecordingController(
        IHistoricRecordingsService historicRecordingsService,
        IBlobStorageService blobStorageService,
        IAutoSyncService autoSyncService,
        ApplicationDbContext dbContext)
    {
        _historicRecordingsService = historicRecordingsService;
        _blobStorageService = blobStorageService;
        _autoSyncService = autoSyncService;
        _dbContext = dbContext;
    }

    [HttpPost("manual")]
    public async Task<IActionResult> SynchronizeRecordings([FromBody] SyncDateRangeDto dateRange)
    {
        try
        {
            if (dateRange == null || dateRange.StartDate == default || dateRange.EndDate == default)
            {
                return BadRequest(new { message = "Start date and end date are required" });
            }

            await SynchronizeRecordingsAsync(dateRange.StartDate, dateRange.EndDate);
            return Ok(new { message = "Sync complete" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
        }
    }

    private async Task SynchronizeRecordingsAsync(DateTime fromDate, DateTime toDate)
    {
        await _autoSyncService.SynchronizeRecordingsAsync(fromDate, toDate);
    }
}
