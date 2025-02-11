using backend.Data.Models;
using backend.Data.Seeds;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, string>
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        try
        {
            // Check if database exists, if not, create it
            var databaseCreator = Database.GetService<IRelationalDatabaseCreator>();
            if (!databaseCreator.Exists())
            {
                databaseCreator.Create();
            }

            // Check if tables exist
            var tables = databaseCreator.GetTables();
            var tablesExist = tables.Any(t => t.Name == "AspNetUsers" || t.Name == "AspNetRoles");

            if (!tablesExist)
            {
                // Tables don't exist, create them
                databaseCreator.CreateTables();
            }

            // Always seed data, EF Core will handle updates for existing records
            AuditEventSeeder.Seed(modelBuilder);
            CallTypeSeeder.Seed(modelBuilder);

            var adminUserName = _configuration["AdminCredentials:UserName"];
            var adminPassword = _configuration["AdminCredentials:Password"];

            if (string.IsNullOrEmpty(adminUserName) || string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("Admin credentials not found in configuration.");
            }

            UserRoleSeeder.Seed(modelBuilder, adminUserName, adminPassword);
        }
        catch (Exception ex)
        {
            // Log the error but allow the migration to continue
            Console.WriteLine($"Error in OnModelCreating: {ex.Message}");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suppress the warning about model changes
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    public DbSet<CallType> CallTypes { get; set; }
    public DbSet<SyncedRecording> SyncedRecordings { get; set; }
}