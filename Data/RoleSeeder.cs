using Microsoft.AspNetCore.Identity;

namespace backend.Data;

public static class RoleSeeder
{
    public static void Seed(RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.RoleExistsAsync("Admin").Result)
        {
            IdentityRole role = new IdentityRole
            {
                Name = "Admin"
            };

            IdentityResult roleResult = roleManager.CreateAsync(role).Result;
        }

        if (!roleManager.RoleExistsAsync("User").Result)
        {
            IdentityRole role = new IdentityRole
            {
                Name = "User"
            };

            IdentityResult roleResult = roleManager.CreateAsync(role).Result;
        }
    }
}