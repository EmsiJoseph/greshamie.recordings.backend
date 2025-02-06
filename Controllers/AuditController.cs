using System;
using System.Threading.Tasks;
using backend.Services;
using backend.Services.Audits;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Retrieves all audit entries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _auditService.GetAuditEntriesAsync();
            return Ok(entries);
        }

        /// <summary>
        /// Retrieves a single audit entry by its ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var entry = await _auditService.GetAuditEntryByIdAsync(id);
                return Ok(entry);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves audit entries filtered by event type.
        /// Example: GET /api/audit/by-event?eventType=1
        /// </summary>
        [HttpGet("by-event")]
        public async Task<IActionResult> GetByEventType([FromQuery] int eventType)
        {
            var entries = await _auditService.GetAuditEntriesByEventTypeAsync(eventType);
            return Ok(entries);
        }

        /// <summary>
        /// Retrieves audit entries based on optional filter parameters.
        /// Example: GET /api/audit/filter?eventType=1&userId=abc123&startDate=2025-01-01&endDate=2025-02-01
        /// </summary>
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered(
            [FromQuery] int? eventType,
            [FromQuery] string? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var entries = await _auditService.GetAuditEntriesFilteredAsync(eventType, userId, startDate, endDate);
            return Ok(entries);
        }
    }
}
