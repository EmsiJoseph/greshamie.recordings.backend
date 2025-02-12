using backend.DTOs;
using backend.DTOs.Audit;

namespace backend.Services.Audits
{
    public interface IAuditService
    {
        /// <summary>
        /// Logs an audit entry for a given user and event.
        /// </summary>
        /// <param name="userId">The identifier of the user related to the audit event.</param>
        /// <param name="eventId">The identifier for the audit event.</param>
        /// <param name="recordId">Optional identifier for the related recording.</param>
        /// <param name="details">Optional details about the event.</param>
        Task LogAuditEntryAsync(string userId, int eventId, string? recordId = null, string? details = null);

        /// <summary>
        /// Retrieves all audit entries.
        /// </summary>
        /// <returns>A list of audit entries.</returns>
        Task<List<AuditEntryDto>> GetAuditEntriesAsync();

        /// <summary>
        /// Retrieves a specific audit entry by its identifier.
        /// </summary>
        /// <param name="id">The audit entry identifier.</param>
        /// <returns>The audit entry.</returns>
        Task<AuditEntryDto> GetAuditEntryByIdAsync(int id);

        /// <summary>
        /// Retrieves audit entries based on optional filter parameters.
        /// If a parameter is null, that filter is ignored.
        /// </summary>
        /// <param name="eventType">Optional event type filter.</param>
        /// <param name="userId">Optional user identifier filter.</param>
        /// <param name="startDate">Optional start date filter for the timestamp.</param>
        /// <param name="endDate">Optional end date filter for the timestamp.</param>
        /// <returns>A list of audit entries that match the provided filters.</returns>
        Task<List<AuditEntryDto>> GetAuditEntriesFilteredAsync(
            int? eventType = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Retrieves paginated audit entries based on optional search and pagination parameters.
        /// </summary>
        /// <param name="search">Optional search term.</param>
        /// <param name="pagination">Pagination parameters.</param>
        /// <returns>A paginated list of audit entries.</returns>
        Task<PagedResponseDto<AuditEntryDto>> GetAuditEntriesAsync(string? search, PaginationDto pagination);
    }
}
