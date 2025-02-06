using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAuditEntryAsync(string userId, int eventId, string? details = null)
        {
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

        public async Task<List<AuditEntry>> GetAuditEntriesAsync()
        {
            return await _context.AuditEntries
                .Include(ae => ae.User)  // Include User data if needed
                .Include(ae => ae.Event) // Include AuditEvent data if needed
                .ToListAsync();
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
    }
}