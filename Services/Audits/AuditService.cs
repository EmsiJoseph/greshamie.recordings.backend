using backend.Data;
using backend.Exceptions;
using backend.Models;
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

        public async Task<List<AuditEntryDto>> GetAuditEntriesAsync()
        {
            try
            {
                var entries = await _context.AuditEntries
                    .Include(ae => ae.User)
                    .Include(ae => ae.Event)
                    .ToListAsync();
                return entries.Select(ae => new AuditEntryDto
                {
                    Id = ae.Id,
                    Username = ae.User.UserName,
                    EventName = ae.Event.Name,
                    Timestamp = ae.Timestamp,
                    Details = ae.Details,
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to retrieve audit entries: {ex.Message}", 500);
            }
        }

        public async Task<AuditEntryDto> GetAuditEntryByIdAsync(int id)
        {
            try
            {
                var entry = await _context.AuditEntries
                    .Include(ae => ae.User)
                    .Include(ae => ae.Event)
                    .FirstOrDefaultAsync(ae => ae.Id == id);

                if (entry == null)
                {
                    throw new ServiceException($"Audit entry with ID {id} not found.");
                }

                return new AuditEntryDto
                {
                    Id = entry.Id,
                    Username = entry.User.UserName,
                    EventName = entry.Event.Name,
                    Timestamp = entry.Timestamp,
                    Details = entry.Details,
                };
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to retrieve audit entry: {ex.Message}", 500);
            }
        }

        public async Task<List<AuditEntryDto>> GetAuditEntriesFilteredAsync(
            int? eventType = null,
            string? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var entries = _context.AuditEntries
                    .Include(ae => ae.User)
                    .Include(ae => ae.Event)
                    .AsQueryable();

                if (eventType.HasValue)
                {
                    entries = entries.Where(ae => ae.EventId == eventType.Value);
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    entries = entries.Where(ae => ae.UserId == userId);
                }

                if (startDate.HasValue)
                {
                    entries = entries.Where(ae => ae.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    entries = entries.Where(ae => ae.Timestamp <= endDate.Value);
                }

                return await entries.Select(ae => new AuditEntryDto
                {
                    Id = ae.Id,
                    Username = ae.User.UserName,
                    EventName = ae.Event.Name,
                    Timestamp = ae.Timestamp,
                    Details = ae.Details,
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ServiceException($"Failed to retrieve audit entries: {ex.Message}", 500);
            }
        }
    }
}