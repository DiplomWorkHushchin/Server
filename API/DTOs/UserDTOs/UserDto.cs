using API.Entities;

namespace API.DTOs.UserDTOs;

public class UserDto
{
    public required string UserRoles { get; set; }
    public required string UserName { get; set; }
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FatherName { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? UserPhoto { get; set; }
}
