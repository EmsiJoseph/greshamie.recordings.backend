using backend.Constants;
using backend.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public static class UserRoleSeeder
    {
        public static void SeedUserRole(this ModelBuilder modelBuilder, string adminUserName, string adminPassword,
            string adminRoleIdFromConfig, string userRoleIdFromConfig, string adminUserIdFromConfig)
        {
            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentException("Admin password cannot be null or empty", nameof(adminPassword));
            }

            var adminRoleId = adminRoleIdFromConfig;
            var userRoleId = userRoleIdFromConfig;
            var adminUserId = adminUserIdFromConfig;

            // Define roles without HasData to avoid duplicate key issues`
            var adminRole = new Role
            {
                Id = adminRoleId,
                Name = RolesConstants.Admin,
                NormalizedName = RolesConstants.Admin.ToUpperInvariant(),
                Description = "Administrator role with full access",
                Level = 100
            };

            var userRole = new Role
            {
                Id = userRoleId,
                Name = RolesConstants.User,
                NormalizedName = RolesConstants.User.ToUpperInvariant(),
                Description = "Standard user role with limited access",
                Level = 90
            };

            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = adminUserId,
                UserName = adminUserName,
                NormalizedUserName = adminUserName.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = hasher.HashPassword(null, adminPassword)
            };

            // Use InsertData instead of HasData for more control
            modelBuilder.Entity<Role>().HasData(adminRole);
            modelBuilder.Entity<Role>().HasData(userRole);
            modelBuilder.Entity<User>().HasData(adminUser);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );
        }
    }
}