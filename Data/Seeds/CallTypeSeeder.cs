using backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeds
{
    public static class CallTypeSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CallType>().HasData(
                new CallType
                {
                    Id = 0,
                    Name = "Incoming",
                    Description = "An inbound call."
                },
                new CallType
                {
                    Id = 1,
                    Name = "Outgoing",
                    Description = "An outbound call."
                },
                new CallType
                {
                    Id = 2,
                    Name = "Internal",
                    Description = "An internal call."
                }
            );
        }
    }
}