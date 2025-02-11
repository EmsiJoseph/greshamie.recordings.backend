using System;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Constants;
using backend.Services.Audits;
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
        private readonly IAuditService _auditService;

        public SyncRecordingController(ISyncService syncService, ILogger<SyncRecordingController> logger, IAuditService auditService)
        {
            _syncService = syncService;
            _logger = logger;
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        // POST: api/sync/synchronize?fromDate=2025-01-01&toDate=2025-01-31
        [HttpPost("synchronize")]
        public async Task<IActionResult> Synchronize([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try 
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not found" });
                }

                await _syncService.SynchronizeRecordingsAsync(fromDate, toDate);

                // Log the recording sync event using your audit service with date range
                string logMessage = $"User synced recordings from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";
                await _auditService.LogAuditEntryAsync(userId, AuditEventTypes.ManualSync, logMessage);

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