using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Shine.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Drop existing database to clean up constraints
                context.Database.EnsureDeleted();
                
                // Create new database and apply migrations
                context.Database.Migrate();
                
                logger.LogInformation("Database migrated successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database");
                throw;
            }
        }
    }
}