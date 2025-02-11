using backend.Data.Models;
using backend.Data.Seeds;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.SeedAuditEvent();
        builder.SeedCallType();

        var adminUserName = _configuration["AdminCredentials:UserName"];
        var adminPassword = _configuration["AdminCredentials:Password"];

        if (string.IsNullOrEmpty(adminUserName) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin credentials not found in configuration.");
        }

        builder.SeedUserRole(adminUserName, adminPassword);

        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.Event)
            .WithMany(e => e.AuditEntries)
            .HasForeignKey(ae => ae.EventId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.User)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(ae => ae.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.Recording)
            .WithMany(r => r.AuditEntries)
            .HasForeignKey(ae => ae.RecordId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .EnableSensitiveDataLogging() // For debugging
            .EnableDetailedErrors() // For debugging
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                // Ignore duplicate table warnings
                warnings.Ignore(CoreEventId.DuplicateDependentEntityTypeInstanceWarning);
            });
    }

    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    public DbSet<CallType> CallTypes { get; set; }
    public DbSet<SyncedRecording> SyncedRecordings { get; set; }
}