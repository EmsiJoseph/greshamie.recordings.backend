using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using backend.Data;
using backend.Data.Models;
using backend.Services.Sync;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlobRecordingController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISyncService _syncService;
        private readonly ILogger<BlobRecordingController> _logger;

        public BlobRecordingController(
            ApplicationDbContext dbContext,
            ISyncService syncService,
            ILogger<BlobRecordingController> logger)
        {
            _dbContext = dbContext;
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a synced recording from the database. If it doesn’t exist,
        /// it calls the sync service to export and sync the recording.
        /// </summary>
        private async Task<SyncedRecording> GetSyncedRecordingAsync(string recordingId)
        {
            // Attempt to get the synced recording from the database.
            var syncedRecording = await _dbContext.SyncedRecordings.FindAsync(recordingId);
            if (syncedRecording == null)
            {
                // If not found, attempt to sync the recording.
                await _syncService.SyncRecordingByIdAsync(recordingId);
                syncedRecording = await _dbContext.SyncedRecordings.FindAsync(recordingId);
            }
            return syncedRecording;
        }

        // GET: api/blobrecording/{recordingId}/stream
        [HttpGet("{recordingId}/stream")]
        public async Task<IActionResult> GetStreamingUrl(string recordingId)
        {
            try
            {
                var syncedRecording = await GetSyncedRecordingAsync(recordingId);
                if (syncedRecording == null)
                {
                    return NotFound(new { Message = "Recording not found." });
                }
                return Ok(new { StreamingUrl = syncedRecording.StreamingUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving streaming URL for recording {RecordingId}", recordingId);
                return StatusCode(500, new { Message = "Error retrieving streaming URL", Error = ex.Message });
            }
        }

        // GET: api/blobrecording/{recordingId}/download
        [HttpGet("{recordingId}/download")]
        public async Task<IActionResult> GetDownloadUrl(string recordingId)
        {
            try
            {
                var syncedRecording = await GetSyncedRecordingAsync(recordingId);
                if (syncedRecording == null)
                {
                    return NotFound(new { Message = "Recording not found." });
                }
                return Ok(new { DownloadUrl = syncedRecording.DownloadUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving download URL for recording {RecordingId}", recordingId);
                return StatusCode(500, new { Message = "Error retrieving download URL", Error = ex.Message });
            }
        }
    }
}
