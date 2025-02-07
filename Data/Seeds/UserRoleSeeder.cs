using backend.Constants;
using backend.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Data.Seeds
{
    public static class RoleSeeder
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Name = RolesConstants.Admin, NormalizedName = RolesConstants.Admin.ToUpperInvariant(),
                    Description = "Administrator of Gresham Recordings", Level = 90
                },
                new Role
                {
                    Name = RolesConstants.User, NormalizedName = RolesConstants.User.ToUpperInvariant(),
                    Description = "User of Gresham Recordings", Level = 90
                });
        }
    }
}