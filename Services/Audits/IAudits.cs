using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;

namespace backend.Services
{
    public interface IAuditService
    {
        Task LogAuditEntryAsync(string userId, int eventId, string? details = null);
        Task<List<AuditEntry>> GetAuditEntriesAsync();
        Task<AuditEntry> GetAuditEntryByIdAsync(int id);
    }
}