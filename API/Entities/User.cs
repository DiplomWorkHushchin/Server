using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class User : IdentityUser<int>
{
    public ICollection<UserRole> UserRoles { get; set; } = [];
    [MinLength(2)]
    public required string FirstName { get; set; } = "";
    [MinLength(2)]
    public required string LastName { get; set; } = "";
    [MinLength(2)]
    public required string FatherName { get; set; } = "";
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiryTime { get; set; }
}
