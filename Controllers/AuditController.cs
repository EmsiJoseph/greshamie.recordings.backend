using backend.DTOs;
using backend.Exceptions;
using backend.Services.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using backend.Constants;
using backend.DTOs.Audit;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(ApiVersionConstants.VersionString)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuditController(IAuditService auditService, ILogger<AuditController> logger)
        : ControllerBase
    {
        private readonly IAuditService _auditService =
            auditService ?? throw new ArgumentNullException(nameof(auditService));

        private readonly ILogger<AuditController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Gets all audit logs with optional filtering and pagination.
        /// Use this to see who did what and when in the system.
        /// </summary>
        [OutputCache(Duration = 60, VaryByQueryKeys = ["AuditRequestDto"])]
        [HttpGet("")]
        public async Task<IActionResult> GetAll(
            [FromQuery] AuditRequestDto dto)
        {
            try
            {
                var results = await _auditService.GetAuditEntriesAsync(dto);
                return Ok(results);
            }
            catch (ServiceException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAll");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
}