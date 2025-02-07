using backend.Data;
using backend.Exceptions;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Audits   
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogAuditEntryAsync(string userId, int eventId, string? details = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
                }

                var auditEntry = new AuditEntry
                {
                    UserId = userId,
                    EventId = eventId,
                    Timestamp = DateTime.UtcNow,
                    Details = details
                };

                await _context.AuditEntries.AddAsync(auditEntry);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new ServiceException("Failed to save audit entry to database", 500);
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Unexpected error logging audit entry: {ex.Message}", 500);
            }
        }

        public async Task<List<AuditEntry>> GetAuditEntriesAsync()
        {
            try
            {
                return await _context.AuditEntries
                    .Include(ae => ae.User)
                    .Include(ae => ae.Event)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to retrieve audit entries: {ex.Message}", 500);
            }
        }

        public async Task<AuditEntry> GetAuditEntryByIdAsync(int id)
        {
            var auditEntry = await _context.AuditEntries
                .Include(ae => ae.User)
                .Include(ae => ae.Event)
                .FirstOrDefaultAsync(ae => ae.Id == id);

            if (auditEntry == null)
            {
                throw new KeyNotFoundException($"Audit entry with ID {id} not found.");
            }

            return auditEntry;
        }

        public async Task<List<AuditEntry>> GetAuditEntriesByEventTypeAsync(int eventType)
        {
            return await _context.AuditEntries
                .Include(ae => ae.User)
                .Include(ae => ae.Event)
                .Where(ae => ae.EventId == eventType)
                .ToListAsync();
        }

        public async Task<List<AuditEntry>> GetAuditEntriesFilteredAsync(
            int? eventType = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            // Start with the base query.
            IQueryable<AuditEntry> query = _context.AuditEntries
                .Include(ae => ae.User)
                .Include(ae => ae.Event);

            // Apply filters only if they are provided.
            if (eventType.HasValue)
            {
                query = query.Where(ae => ae.EventId == eventType.Value);
            }

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(ae => ae.UserId == userId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(ae => ae.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(ae => ae.Timestamp <= endDate.Value);
            }

            return await query.ToListAsync();
        }
    }
}
