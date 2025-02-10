using backend.Data.Models;
using backend.Data.Seeds;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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