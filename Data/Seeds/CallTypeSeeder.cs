using backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeds
{
    public static class CallTypeSeeder
    {
        public static void SeedCallType(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallType>().HasData(
                new CallType
                {
                    Id = 1,
                    Name = "incoming",
                    NormalizedName = "INCOMING",
                    IdFromClarify = 0,
                    Description = "An inbound call."
                },
                new CallType
                {
                    Id = 2,
                    Name = "outgoing",
                    NormalizedName = "OUTGOING",
                    IdFromClarify = 1,
                    Description = "An outbound call."
                },
                new CallType
                {
                    Id = 3,
                    Name = "internal",
                    NormalizedName = "INTERNAL",
                    IdFromClarify = 2,
                    Description = "An internal call."
                }
            );
        }
    }
}