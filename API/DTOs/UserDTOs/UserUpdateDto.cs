namespace API.DTOs.UserDTOs;

public class UserUpdateDto
{
    public required string FirstName{ get; set; }
    public required string LastName { get; set; }
    public string? FatherName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public required string Username { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}
