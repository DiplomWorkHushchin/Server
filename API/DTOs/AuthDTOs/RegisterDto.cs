namespace API.DTOs.AuthDTOs;

public class RegisterDto
{
    public required string GroupName { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? FatherName { get; set; }
    public string? Password { get; set; }
    public required string Role { get; set; }
    public required string Email { get; set; }
}
