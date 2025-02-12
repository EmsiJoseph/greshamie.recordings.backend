using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using backend.Data;
using backend.Data.Models;
using backend.DTOs;
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
        private async Task<SyncedRecording> GetSyncedRecordingAsync(RecordingDto dto)
        {
            // Attempt to get the synced recording from the database.
            var syncedRecording = await _dbContext.SyncedRecordings.FindAsync(dto.Id);
            if (syncedRecording == null)
            {
                // If not found, attempt to sync the recording.
                await _syncService.SyncRecordingByObjectAsync(dto);
                syncedRecording = await _dbContext.SyncedRecordings.FindAsync(dto.Id);
            }
            return syncedRecording;
        }

        // GET: api/blobrecording/{recordingId}/stream
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
                return Ok(new { StreamingUrl = syncedRecording.StreamingUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving streaming URL", Error = ex.Message });
            }
        }

        // GET: api/blobrecording/{recordingId}/download
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
                return Ok(new { DownloadUrl = syncedRecording.DownloadUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error retrieving download URL", Error = ex.Message });
            }
        }
    }
}
