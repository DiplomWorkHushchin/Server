using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedData(UserManager<User> userManager, 
        RoleManager<Role> roleManager)
    {
        var roles = new List<Role>
        {
            new() { Name = "Admin" },
            new() { Name = "Student" },
            new() { Name = "Curator" },
            new() { Name = "Teacher" },
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var admin = new User
        {
            UserName = "admin"
        };

        // Admin password here only for dev (Update logic to get from env sooner)
        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
