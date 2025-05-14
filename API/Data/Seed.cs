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
            FatherName = "Admin",
            FirstName = "ADMIN",
            LastName = "",
            PhoneNumber = "",
            GroupId = null,
            UserRoles = new List<UserRole>
            {
                new() { Role = roles[0] }
            }

        };

        // Admin password here only for dev (Update logic to get from env sooner)
        await userManager.CreateAsync(admin, "Pa$$w0rd");

        var teachers = new List<User>
        {
            new() {
                UserName = "teacher1",
                Email = "teacher1@example.com",
                FirstName = "Alice",
                LastName = "Johnson",
                FatherName = "Eduardovna",
                UserRoles = new List<UserRole> { new() { Role = roles[3] } }
            },
            new() {
                UserName = "teacher2",
                Email = "teacher2@example.com",
                FirstName = "Bob",
                LastName = "Smith",
                FatherName = "Ivanovich",
                UserRoles = new List<UserRole> { new() { Role = roles[3] } }
            }
        };

        foreach (var teacher in teachers)
        {
                await userManager.CreateAsync(teacher, "Pa$$w0rd");
        }

        var students = new List<User>
        {
            new() {
                UserName = "student1",
                Email = "student1@example.com",
                FirstName = "Charlie",
                LastName = "Brown",
                FatherName = "Petrovich",
                UserRoles = new List<UserRole> { new() { Role = roles[1] } }
            },
            new() {
                UserName = "student2",
                Email = "student2@example.com",
                FirstName = "Dana",
                LastName = "White",
                FatherName = "Sergeevna",
                UserRoles = new List<UserRole> { new() { Role = roles[1] } }
            },
            new() {
                UserName = "student3",
                Email = "student3@example.com",
                FirstName = "Evan",
                LastName = "Taylor",
                FatherName = "Nikolaevich",
                UserRoles = new List<UserRole> { new() { Role = roles[1] } }
            },
            new() {
                UserName = "student4",
                Email = "student4@example.com",
                FirstName = "Fiona",
                LastName = "Lee",
                FatherName = "Alexandrovna",
                UserRoles = new List<UserRole> { new() { Role = roles[1] } }
            },
            new() {
                UserName = "student5",
                Email = "student5@example.com",
                FirstName = "George",
                LastName = "Martin",
                FatherName = "Yurievich",
                UserRoles = new List<UserRole> { new() { Role = roles[1] } }
            }
        };

        foreach (var student in students)
        {
                await userManager.CreateAsync(student, "Pa$$w0rd");
        }
    }
}
