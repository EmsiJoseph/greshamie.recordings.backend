using backend.Constants.Audit;
using backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeds
{
    public static class AuditEventTypeSeeder
    {
        public static void SeedAuditEventType(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditEventType>().HasData(
                new AuditEventType
                {
                    Id = AuditEventTypesConstants.RecordingId,
                    Name = "Recording",
                    NormalizedName = "RECORDING",
                    Description = "Events related to call recordings."
                },
                new AuditEventType
                {
                    Id = AuditEventTypesConstants.SessionId,
                    Name = "Session",
                    NormalizedName = "SESSION",
                    Description = "Events related to user sessions."
                }
            );
        }
    }
}