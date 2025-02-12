using backend.DTOs;
using backend.Exceptions;
using backend.Services.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using backend.Constants;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(ApiVersionConstants.VersionString)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuditController(IAuditService auditService, ILogger<AuditController> logger)
        : ControllerBase
    {
        private readonly IAuditService _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        private readonly ILogger<AuditController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [OutputCache(Duration = 60, VaryByQueryKeys = ["search", "pageOffset", "pageSize"])]
        [HttpGet("")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int pageOffset = 0,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation(
                    "Searching audit entries with search term: {Search}, PageOffset: {Offset}, Size: {Size}",
                    search, pageOffset, pageSize);

                var pagination = new PaginationDto
                {
                    PageOffset = pageOffset,
                    PageSize = pageSize
                };
                // TODO: Implement filter by date time range

                var results = await _auditService.GetAuditEntriesAsync(search, pagination);
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