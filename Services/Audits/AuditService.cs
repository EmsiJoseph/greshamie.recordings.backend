using backend.Data;
using backend.Data.Models;
using backend.Exceptions;
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

                // Validate user exists
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new ServiceException($"User with ID {userId} not found", 404);
                }

                // Validate event exists
                var auditEvent = await _context.AuditEvents.FindAsync(eventId);
                if (auditEvent == null)
                {
                    throw new ServiceException($"Audit event with ID {eventId} not found", 404);
                }

                var auditEntry = new AuditEntry
                {
                    UserId = userId,
                    EventId = eventId,
                    Timestamp = DateTime.UtcNow,
                    Details = details
                };

                _context.AuditEntries.Add(auditEntry);
                
                // Enable change tracking explicitly
                _context.ChangeTracker.DetectChanges();
                
                var changes = await _context.SaveChangesAsync();
                
                if (changes == 0)
                {
                    throw new ServiceException("No changes were saved to the database", 500);
                }

                Console.WriteLine($"Audit entry logged successfully: {auditEntry.Id}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error: {ex.InnerException?.Message ?? ex.Message}");
                throw new ServiceException($"Failed to save audit entry: {ex.InnerException?.Message ?? ex.Message}", 500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw;
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