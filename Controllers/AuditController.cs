using System;
using System.Threading.Tasks;
using backend.Exceptions;
using backend.Services;
using backend.Services.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace backend.Controllers
{
    [Authorize]
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
        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "AuditCache" })]
        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var entries = await _auditService.GetAuditEntriesAsync();
                return Ok(entries);
            }
            catch (ServiceException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving audit entries" });
            }
        }

        /// <summary>
        /// Retrieves a single audit entry by its ID.
        /// </summary>
        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "AuditCache" })]
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
            catch (ServiceException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An unexpected error occurred while retrieving the audit entry" });
            }
        }


        /// <summary>
        /// Retrieves audit entries based on optional filter parameters.
        /// Example: GET /api/audit/filter?eventType=1&userId=abc123&startDate=2025-01-01&endDate=2025-02-01
        /// </summary>
        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "AuditCache" })]
        [HttpGet("search")]
        public async Task<IActionResult> SearchAudits(
            [FromQuery] int? eventType,
            [FromQuery] string? userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var entries = await _auditService.GetAuditEntriesFilteredAsync(eventType, userId, startDate, endDate);
                return Ok(entries);
            }
            catch (ServiceException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = "An unexpected error occurred while retrieving filtered audit entries" });
            }
        }
    }
}