using backend.Data.Models;
using backend.DTOs;
using backend.DTOs.Audit;

namespace backend.Services.Audits
{
    public interface IAuditService
    {
        /// <summary>
        /// Records a new audit event in the system.
        /// Helps track who did what and when.
        /// </summary>
        Task LogAuditEntryAsync(AuditEntry entry);

        /// <summary>
        /// Gets a paginated list of audit entries.
        /// Can search across all fields and filter by event type.
        /// </summary>
        /// <returns>A paginated list of audit entries.</returns>
        Task<PagedResponseDto<AuditResponseDto>> GetAuditEntriesAsync(AuditRequestDto dto);
    }
}