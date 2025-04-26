using API.Entities;
using Microsoft.AspNetCore.Identity;

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
            new() { Name = "Guest" },
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        var admin = new User
        {
            UserName = "admin",
            Email = "admin@example.com",
            FatherName = "",
            FirstName = "",
            LastName = "",
            PhoneNumber = "",
            UserRoles = new List<UserRole>
            {
                new() { Role = roles[0] }
            }

        };

        // Admin password here only for dev (Update logic to get from env sooner)
        await userManager.CreateAsync(admin, "Pa$$w0rd");
    }
}
