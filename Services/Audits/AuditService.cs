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

            // Apply event type filter
            if (!string.IsNullOrWhiteSpace(dto.EventType))
            {
                query = dto.EventType.ToUpperInvariant() == "ALL"
                    ? query.Where(x => true)
                    : query.Where(x => EF.Functions.Like(x.Event.Type.Name, dto.EventType));
            }

            // Apply date range filter with proper time boundaries
            if (dto.StartDate.HasValue || dto.EndDate.HasValue)
            {
                var startDate = dto.StartDate?.Date ?? DateTime.MinValue;
                var endDate = dto.EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

                // Ensure start date is not after end date
                if (startDate > endDate)
                {
                    throw new ServiceException("Start date cannot be after end date");
                }

                query = query.Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate);
                
                _logger.LogInformation(
                    "Applying date filter: Start={StartDate:yyyy-MM-dd HH:mm:ss}, End={EndDate:yyyy-MM-dd HH:mm:ss}", 
                    startDate, 
                    endDate);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                query = query.Where(x =>
                    (x.User.UserName != null && EF.Functions.Like(x.User.UserName, $"%{dto.Search}%")) ||
                    EF.Functions.Like(x.Event.Name, $"%{dto.Search}%") ||
                    EF.Functions.Like(x.Event.Type.Name, $"%{dto.Search}%") ||
                    (x.Details != null && EF.Functions.Like(x.Details, $"%{dto.Search}%")) ||
                    (x.RecordId != null && EF.Functions.Like(x.RecordId, $"%{dto.Search}%"))
                );
            }

            // Get total count before applying pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize);

            // Apply ordering and pagination
            var entriesInPage = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip(dto.PageOffSet * dto.PageSize)
                .Take(dto.PageSize)
                .Select(audit => new AuditResponseDto
                {
                    Id = audit.Id,
                    UserName = audit.User.UserName ?? "Unknown",
                    RecordingId = audit.RecordId ?? "N/A",
                    EventName = audit.Event.Name,
                    EventType = audit.Event.Type.Name,
                    Details = audit.Details ?? "No details",
                    Timestamp = audit.Timestamp
                })
                .ToListAsync();

            // Convert to uppercase after data is retrieved
            foreach (var entry in entriesInPage)
            {
                entry.EventName = entry.EventName?.ToUpperInvariant();
                entry.EventType = entry.EventType?.ToUpperInvariant();
            }

            return new PagedResponseDto<AuditResponseDto>
            {
                Items = entriesInPage,
                PageOffSet = dto.PageOffSet,
                PageSize = dto.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
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