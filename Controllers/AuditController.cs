using System;
using System.Threading.Tasks;
using backend.Data.Models;
using backend.DTOs;
using backend.Exceptions;
using backend.Services;
using backend.Services.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using backend.Constants;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion(ApiVersionConstants.VersionString)]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<List<AuditEntryDto>> SearchAndProcessAuditEntries(string? search)
        {
            try
            {
                _logger.LogInformation("Fetching audit entries");
                var entries = await _auditService.GetAuditEntriesAsync();
                
                _logger.LogInformation("Processing {Count} entries with search term: {Search}", 
                    entries?.Count ?? 0, search ?? "null");

                if (entries == null)
                {
                    _logger.LogWarning("GetAuditEntriesAsync returned null");
                    return new List<AuditEntryDto>();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    entries = entries.Where(x =>
                        (!string.IsNullOrEmpty(x.Username) && x.Username.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.RecordingId) && x.RecordingId.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.EventName) && x.EventName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Details) && x.Details.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.EventType) && x.EventType.Contains(search, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                    
                    _logger.LogInformation("Found {Count} entries after search filter", entries.Count);
                }

                return entries;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Service error searching audit entries. Search term: {Search}", search);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching audit entries. Search term: {Search}", search);
                throw new ServiceException($"Failed to retrieve audit entries: {ex.Message}", 500);
            }
        }

        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "search", "pageOffset", "pageSize" })]
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