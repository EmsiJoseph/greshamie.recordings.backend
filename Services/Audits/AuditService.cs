using backend.Data;
using backend.DTOs;
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
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var auditEntry = new AuditDto()
            {
                UserId = userId,
                EventId = eventId,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            await _context.AuditEntries.AddAsync(auditEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditDto>> GetAuditEntriesAsync()
        {
            return await _context.AuditEntries
                .Select(ae => new AuditDto
                {
                    Id = ae.Id,
                    UserId = ae.UserId,
                    EventName = ae.Event.Name, // Get only the event name
                    Timestamp = ae.Timestamp,
                    Details = ae.Details
                })
                .ToListAsync();
        }

        public async Task<AuditDto> GetAuditEntryByIdAsync(int id)
        {
            var auditEntry = await _context.AuditEntries
                .Where(ae => ae.Id == id)
                .Select(ae => new AuditDto
                {
                    Id = ae.Id,
                    UserId = ae.UserId,
                    EventName = ae.Event.Name, // Get only the event name
                    Timestamp = ae.Timestamp,
                    Details = ae.Details
                })
                .FirstOrDefaultAsync();

            if (auditEntry == null)
            {
                throw new KeyNotFoundException($"Audit entry with ID {id} not found.");
            }

            return auditEntry;
        }

        public async Task<List<AuditDto>> GetAuditEntriesByEventTypeAsync(int eventType)
        {
            return await _context.AuditEntries
                .Where(ae => ae.EventId == eventType)
                .Select(ae => new AuditDto
                {
                    Id = ae.Id,
                    UserId = ae.UserId,
                    EventName = ae.Event.Name, // Get only the event name
                    Timestamp = ae.Timestamp,
                    Details = ae.Details
                })
                .ToListAsync();
        }

        public async Task<List<AuditDto>> GetAuditEntriesFilteredAsync(
            int? eventType = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<Models.AuditEntry> query = _context.AuditEntries;

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

            return await query
                .Select(ae => new AuditDto
                {
                    Id = ae.Id,
                    UserId = ae.UserId,
                    EventName = ae.Event.Name, // Get only the event name
                    Timestamp = ae.Timestamp,
                    Details = ae.Details
                })
                .ToListAsync();
        }
    }
}
