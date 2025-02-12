using backend.Data.Models;
using backend.Data.Seeds;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // These seeds will always run
        builder.SeedAuditEvent();
        builder.SeedCallType();
        builder.SeedAuditEventType();

        // Always configure the user role seed data, but don't check tables here
        var adminUserName = _configuration["AdminCredentials:UserName"];
        var adminPassword = _configuration["AdminCredentials:Password"];
        builder.SeedUserRole(adminUserName, adminPassword);

        // Entity configurations
        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.Event)
            .WithMany(e => e.AuditEntries)
            .HasForeignKey(ae => ae.EventId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.User)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(ae => ae.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.Entity<AuditEntry>()
            .HasOne(ae => ae.Recording)
            .WithMany(r => r.AuditEntries)
            .HasForeignKey(ae => ae.RecordId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Entity<AuditEvent>()
            .HasOne(ae => ae.Type)
            .WithMany(et => et.Events)
            .HasForeignKey(ae => ae.TypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();
    }

    /// <summary>
    /// Checks if identity tables are empty and needs seeding
    /// </summary>
    public bool NeedsIdentitySeeding()
    {
        return !Users.Any() && !Roles.Any() && !Set<IdentityUserRole<string>>().Any();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                warnings.Ignore(CoreEventId.DuplicateDependentEntityTypeInstanceWarning);
            });
    }

    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    public DbSet<CallType> CallTypes { get; set; }
    public DbSet<SyncedRecording> SyncedRecordings { get; set; }

    public DbSet<AuditEventType> AuditEventTypes { get; set; }
}