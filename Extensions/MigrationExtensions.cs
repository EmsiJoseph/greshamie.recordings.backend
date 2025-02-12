using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Check if any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
            }

            // After migration, check if we need to seed identity data
            if (context.NeedsIdentitySeeding())
            {
                logger.LogInformation("Performing initial identity data seeding...");
                context.Database.OpenConnection();
                try
                {
                    // Disable constraint checks
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.AspNetUsers ON");
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.AspNetRoles ON");

                    // Save changes to trigger the seeding
                    context.SaveChanges();

                    // Re-enable constraint checks
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.AspNetUsers OFF");
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.AspNetRoles OFF");

                    logger.LogInformation("Initial identity data seeding completed.");
                }
                finally
                {
                    context.Database.CloseConnection();
                }
            }

            logger.LogInformation("Database is up to date.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}