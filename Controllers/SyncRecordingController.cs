using System;
using System.Threading.Tasks;
using backend.Services.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SyncRecordingController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly ILogger<SyncRecordingController> _logger;

        public SyncRecordingController(ISyncService syncService, ILogger<SyncRecordingController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        // POST: api/sync/synchronize?fromDate=2025-01-01&toDate=2025-01-31
        [HttpPost("synchronize")]
        public async Task<IActionResult> Synchronize([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try 
            {
                await _syncService.SynchronizeRecordingsAsync(fromDate, toDate);
                return Ok(new { Message = "Synchronization completed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Synchronization failed.");
                return StatusCode(500, new { Message = "Error synchronizing recordings.", Error = ex.Message });
            }
        }
    }
}