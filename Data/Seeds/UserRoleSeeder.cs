using System;
using backend.Constants;
using backend.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeds
{
    public static class UserRoleSeeder
    {
        public static void SeedUserRole(this ModelBuilder builder, string? adminUsername, string? adminPassword)
        {
            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminPassword))
            {
                throw new ArgumentException("Admin credentials are not configured properly");
            }

            builder.Entity<Role>().HasData(
                new Role
                {
                    Id = "1",
                    Name = RolesConstants.Admin,
                    NormalizedName = RolesConstants.Admin.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Description = "Administrator role"
                },
                new Role
                {
                    Id = "2",
                    Name = RolesConstants.User,
                    NormalizedName = RolesConstants.User.ToUpper(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Description = "User role"
                }
            );

            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = "1",
                UserName = adminUsername,
                NormalizedUserName = adminUsername.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

            builder.Entity<User>().HasData(adminUser);

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "1",
                    UserId = "1"
                }
            );
        }
    }
}