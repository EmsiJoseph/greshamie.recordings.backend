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

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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
                var entries = await _auditService.GetAuditEntriesAsync();
                
                if (entries == null)
                {
                    _logger.LogWarning("GetAuditEntriesAsync returned null");
                    return new List<AuditEntryDto>();
                }

                if (!string.IsNullOrEmpty(search))
                {
                    entries = entries.Where(x =>
                        (x.Username?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.RecordingId?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.EventName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (x.Details?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                return entries;
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error searching audit entries. Search term: {Search}", search);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching audit entries. Search term: {Search}", search);
                throw new ServiceException($"Error retrieving audit entries: {ex.Message}", 500);
            }
        }

        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "AuditCache" })]
        [HttpGet("")]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            try
            {
                _logger.LogInformation("Searching audit entries with search term: {Search}", search);
                var results = await SearchAndProcessAuditEntries(search);
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