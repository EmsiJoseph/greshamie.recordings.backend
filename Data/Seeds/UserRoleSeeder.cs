using backend.Constants;
using backend.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public static class UserRoleSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder, string adminUserName, string adminPassword)
        {
            if (string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentException("Admin password cannot be null or empty", nameof(adminPassword));
            }

            var adminRoleId = Guid.NewGuid().ToString();
            var adminUserId = Guid.NewGuid().ToString();

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = adminRoleId, Name = RolesConstants.Admin,
                    NormalizedName = RolesConstants.Admin.ToUpperInvariant(),
                    Description = "Administrator", Level = 100
                },
                new Role
                {
                    Name = RolesConstants.User, NormalizedName = RolesConstants.User.ToUpperInvariant(),
                    Description = "User", Level = 90
                });

            var hasher = new PasswordHasher<User>();

            var adminUser = new User
            {
                Id = adminUserId,
                UserName = adminUserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PasswordHash = hasher.HashPassword(null, adminPassword)
            };

            modelBuilder.Entity<User>().HasData(adminUser);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = adminUserId,
                RoleId = adminRoleId
            });
        }
    }
}