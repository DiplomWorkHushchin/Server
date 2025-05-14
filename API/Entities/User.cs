using API.Entities.Courses;
using API.Extensions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Users")]
public class User : IdentityUser<int>
{
    [MinLength(1)]
    public required ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    [MinLength(2)]
    public required string FirstName { get; set; } = "";
    [MinLength(2)]
    public required string LastName { get; set; } = "";
    [MinLength(2)]
    public required string FatherName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    public ICollection<UserPhoto> Photos { get; set; } = new List<UserPhoto>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public int? GroupId { get; set; }
    public Group? Group { get; set; } = null!;


    public ICollection<CourseInstructor> TeachingCourses { get; set; } = new List<CourseInstructor>();
    public ICollection<CourseStudent> EnrolledCourses { get; set; } = new List<CourseStudent>();


    public int GetAge()
    {
        return DateOfBirth.CalculateAge();
    }
}
