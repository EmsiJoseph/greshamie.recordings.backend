using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, RoleManager<IdentityRole> roleManager) :
        base(options)
    {
        _roleManager = roleManager;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        AuditEventSeeder.Seed(modelBuilder);
        CallTypeSeeder.Seed(modelBuilder);
        RoleSeeder.Seed(_roleManager);
    }

    public DbSet<Models.AuditEntry> AuditEntries { get; set; }
    public DbSet<AuditEvent> AuditEvents { get; set; }
    public DbSet<CallType> CallTypes { get; set; }
}