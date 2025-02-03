using backend.Models;

namespace backend.Services.Audit;

public interface IAuditService
{
    Task LogAction(string userId, string action, string resourceId);
    Task<IEnumerable<AuditEntry>> GetAuditLogs(DateTime from, DateTime to);
}