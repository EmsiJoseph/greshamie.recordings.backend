// ApplicationDbContext.cs
using backend.Data.Models;
using backend.Data.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, string>
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration,
            IWebHostEnvironment environment)
            : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.SeedAuditEvent();
            builder.SeedCallType();
            builder.SeedAuditEventType();

            var adminUserName = _configuration["AdminCredentials:UserName"];
            var adminPassword = _configuration["AdminCredentials:Password"];
            builder.SeedUserRole(adminUserName, adminPassword);

            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasMaxLength(50);
                entity.Property(u => u.ClarifyGoAccessToken).HasColumnType("nvarchar(max)"); // Changed to max
                entity.Property(u => u.RefreshToken).HasColumnType("nvarchar(max)");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
                entity.Property(uc => uc.UserId).HasMaxLength(50));

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(ul => ul.UserId).HasMaxLength(50);
                entity.Property(ul => ul.LoginProvider).HasMaxLength(128);
                entity.Property(ul => ul.ProviderKey).HasMaxLength(128);
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(ur => ur.UserId).HasMaxLength(50);
                entity.Property(ur => ur.RoleId).HasMaxLength(50);
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(ut => ut.UserId).HasMaxLength(50);
                entity.Property(ut => ut.LoginProvider).HasMaxLength(128);
                entity.Property(ut => ut.Name).HasMaxLength(128);
            });

            builder.Entity<Role>(entity =>
                entity.Property(r => r.Id).HasMaxLength(50));

            builder.Entity<SyncedRecording>(entity =>
            {
                entity.ToTable("SyncedRecordings");
                entity.HasKey(sr => sr.Id);
                entity.Property(sr => sr.Id).HasMaxLength(50).IsRequired();
                entity.Property(sr => sr.StreamingUrl).HasMaxLength(512).IsRequired();
                entity.Property(sr => sr.DownloadUrl).HasMaxLength(512).IsRequired();
                entity.Property(sr => sr.IsDeleted).IsRequired();
                entity.Property(sr => sr.RecordingDate).HasColumnType("datetime").IsRequired();
                entity.Property(sr => sr.CreatedAt).HasColumnType("datetime").IsRequired();
                entity.Property(sr => sr.DeletedAt).HasColumnType("datetime").IsRequired(false);
                entity.Property(sr => sr.Caller).HasMaxLength(50);
                entity.Property(sr => sr.Callee).HasMaxLength(50);
                entity.Property(sr => sr.DurationSeconds).IsRequired();
            });

            builder.Entity<AuditEntry>(entity =>
            {
                entity.HasKey(ae => ae.Id);
                entity.Property(ae => ae.UserId).HasMaxLength(50).IsRequired();
                entity.Property(ae => ae.RecordId).HasMaxLength(50);
                entity.HasOne(ae => ae.Event).WithMany(e => e.AuditEntries).HasForeignKey(ae => ae.EventId).OnDelete(DeleteBehavior.NoAction).IsRequired();
                entity.HasOne(ae => ae.User).WithMany(u => u.AuditEntries).HasForeignKey(ae => ae.UserId).OnDelete(DeleteBehavior.NoAction).IsRequired();
                entity.HasOne(ae => ae.Recording).WithMany(r => r.AuditEntries).HasForeignKey(ae => ae.RecordId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            });

            builder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(ae => ae.Id);
                entity.Property(ae => ae.Name).HasMaxLength(50).IsRequired();
                entity.Property(ae => ae.Description).HasMaxLength(100).IsRequired();
                entity.HasOne(ae => ae.Type).WithMany(et => et.Events).HasForeignKey(ae => ae.TypeId).OnDelete(DeleteBehavior.NoAction).IsRequired();
            });

            builder.Entity<AuditEventType>(entity =>
            {
                entity.HasKey(aet => aet.Id);
                entity.Property(aet => aet.Name).HasMaxLength(50).IsRequired();
                entity.Property(aet => aet.NormalizedName).HasMaxLength(50).IsRequired();
                entity.Property(aet => aet.Description).HasMaxLength(100).IsRequired();
            });

            builder.Entity<CallType>(entity =>
            {
                entity.HasKey(ct => ct.Id);
                entity.Property(ct => ct.Name).HasMaxLength(50).IsRequired();
                entity.Property(ct => ct.NormalizedName).HasMaxLength(50).IsRequired();
                entity.Property(ct => ct.Description).HasMaxLength(100).IsRequired();
            });
        }

        public override int SaveChanges()
        {
            LogSyncData();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            LogSyncData();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void LogSyncData()
        {
            foreach (var entry in ChangeTracker.Entries<SyncedRecording>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var recording = entry.Entity;
                    Console.WriteLine($"Saving SyncedRecording Id={recording.Id}: " +
                                      $"StreamingUrl={recording.StreamingUrl} (Length={recording.StreamingUrl?.Length}), " +
                                      $"DownloadUrl={recording.DownloadUrl} (Length={recording.DownloadUrl?.Length}), " +
                                      $"Caller={recording.Caller}, " +
                                      $"Callee={recording.Callee}, " +
                                      $"CreatedAt={recording.CreatedAt}, " +
                                      $"DeletedAt={recording.DeletedAt}, " +
                                      $"DurationSeconds={recording.DurationSeconds}, " +
                                      $"IsDeleted={recording.IsDeleted}, " +
                                      $"RecordingDate={recording.RecordingDate}");
                }
            }
        }

        public bool NeedsIdentitySeeding()
        {
            return !Users.Any() && !Roles.Any() && !Set<IdentityUserRole<string>>().Any();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (_environment.IsDevelopment())
            {
                optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
                optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            }
            optionsBuilder.ConfigureWarnings(warnings =>
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
}