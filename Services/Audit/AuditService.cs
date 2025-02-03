using backend.Models;
using gresham.Data;

namespace backend.Services.Audit;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    
    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAction(string userId, string action, string resourceId)
    {
        _context.AuditLogs.Add(new AuditEntry
        {
            UserId = userId,
            Action = action,
            ResourceId = resourceId,
            Timestamp = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditEntry>> GetAuditLogs(DateTime from, DateTime to)
    {
        return await _context.AuditLogs
            .Where(log => log.Timestamp >= from && log.Timestamp <= to)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }
}