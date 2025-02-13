using backend.Data;
using backend.Data.Models;
using backend.DTOs;
using backend.Exceptions;
using backend.DTOs.Audit;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Audits;

public class AuditService(ApplicationDbContext context, ILogger<AuditService> logger) : IAuditService
{
    private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogger<AuditService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task LogAuditEntryAsync(AuditEntry entry)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entry.UserId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(entry.UserId));
            }

            // Validate user exists
            var user = await _context.Users.FindAsync(entry.UserId);
            if (user == null)
            {
                throw new ServiceException($"User with ID {entry.UserId} not found", 404);
            }

            // Validate event exists
            var auditEvent = await _context.AuditEvents.FindAsync(entry.EventId);
            if (auditEvent == null)
            {
                throw new ServiceException($"Audit event with ID {entry.EventId} not found", 404);
            }

            _context.AuditEntries.Add(entry);

            // Enable change tracking explicitly
            _context.ChangeTracker.DetectChanges();

            var changes = await _context.SaveChangesAsync();

            if (changes == 0)
            {
                throw new ServiceException("No changes were saved to the database");
            }
        }
        catch (DbUpdateException ex)
        {
            throw new ServiceException($"Failed to save audit entry: {ex.InnerException?.Message ?? ex.Message}");
        }
    }


    public async Task<PagedResponseDto<AuditResponseDto>> GetAuditEntriesAsync(AuditRequestDto dto)
    {
        try
        {
            _logger.LogInformation("Fetching audit entries from database with pagination");

            var query = _context.AuditEntries
                .Include(x => x.Event.Type)
                .Include(x => x.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dto.EventType))
            {
                query = query.Where(x => x.Event.Type.Name == dto.EventType);
            }

            if (!string.IsNullOrEmpty(dto.StartDate.ToString()) && !string.IsNullOrEmpty(dto.EndDate.ToString()))
            {
                query = query.Where(x => x.Timestamp >= dto.StartDate && x.Timestamp <= dto.EndDate);
            }

            if (!string.IsNullOrEmpty(dto?.Search))
            {
                query = query.Where(x =>
                    x.User.UserName != null &&
                    x.Details != null &&
                    (x.User.UserName.ToUpperInvariant().Contains(dto.Search.ToUpperInvariant()) ||
                     x.Event.Name.ToUpperInvariant().Contains(dto.Search.ToUpperInvariant()) ||
                     x.Event.Type.Name.ToUpperInvariant().Contains(dto.Search.ToUpperInvariant()) ||
                     x.Details.ToUpperInvariant().Contains(dto.Search.ToUpperInvariant()))
                );
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize);


            var entriesInPage = await query
                .Skip(dto.PageOffSet * dto.PageSize)
                .Take(dto.PageSize)
                .Select(audit => new AuditResponseDto
                {
                    Id = audit.Id,
                    UserName = audit.User.UserName,
                    RecordingId = audit.RecordId ?? "N/A",
                    EventName = audit.Event.Name.ToUpperInvariant(),
                    EventType = audit.Event.Type.Name.ToUpperInvariant(),
                    Details = audit.Details ?? "No details",
                    Timestamp = audit.Timestamp
                })
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();


            return new PagedResponseDto<AuditResponseDto>
            {
                Items = entriesInPage,
                PageOffSet = dto.PageOffSet,
                PageSize = dto.PageSize,
                TotalPages = totalPages,
                HasNext = (dto.PageOffSet + 1) * dto.PageSize < totalCount,
                HasPrevious = dto.PageOffSet > 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit entries from database");
            throw new ServiceException("Failed to retrieve audit entries");
        }
    }
}