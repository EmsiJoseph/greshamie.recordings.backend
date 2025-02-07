using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services
{
    public interface IAuditService
    {
        /// <summary>
        /// Logs an audit entry for a given user and event.
        /// </summary>
        /// <param name="userId">The identifier of the user related to the audit event.</param>
        /// <param name="eventId">The identifier for the audit event.</param>
        /// <param name="details">Optional details about the event.</param>
        Task LogAuditEntryAsync(string userId, int eventId, string? details = null);

        /// <summary>
        /// Retrieves all audit entries.
        /// </summary>
        /// <returns>A list of audit entries.</returns>
        Task<List<AuditEntry>> GetAuditEntriesAsync();

        /// <summary>
        /// Retrieves a specific audit entry by its identifier.
        /// </summary>
        /// <param name="id">The audit entry identifier.</param>
        /// <returns>The audit entry.</returns>
        Task<AuditEntry> GetAuditEntryByIdAsync(int id);

        /// <summary>
        /// Retrieves audit entries filtered by the specified event type.
        /// </summary>
        /// <param name="eventType">The audit event type (using the seeded event ID) to filter by.</param>
        /// <returns>A list of audit entries matching the specified event type.</returns>
        Task<List<AuditEntry>> GetAuditEntriesByEventTypeAsync(int eventType);

        /// <summary>
        /// Retrieves audit entries based on optional filter parameters.
        /// If a parameter is null, that filter is ignored.
        /// </summary>
        /// <param name="eventType">Optional event type filter.</param>
        /// <param name="userId">Optional user identifier filter.</param>
        /// <param name="startDate">Optional start date filter for the timestamp.</param>
        /// <param name="endDate">Optional end date filter for the timestamp.</param>
        /// <returns>A list of audit entries that match the provided filters.</returns>
        Task<List<AuditEntry>> GetAuditEntriesFilteredAsync(
            int? eventType = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}