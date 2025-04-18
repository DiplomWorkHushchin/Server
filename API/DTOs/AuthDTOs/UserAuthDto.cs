using API.DTOs.UserDTOs;

namespace API.DTOs.AuthDTOs;

public class UserAuthDto
{
    public required UserDto User { get; set; }
    public required string Token { get; set; }
}
