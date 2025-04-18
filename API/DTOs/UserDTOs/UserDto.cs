using API.Entities;

namespace API.DTOs.UserDTOs;

public class UserDto
{
    public required string UserName { get; set; }
    public required string UserRoles { get; set; }
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FatherName { get; set; } = "";
}
