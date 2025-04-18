using API.DTOs.AuthDTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AccountControllers;

public class AuthController(UserManager<User> userManager, ITokenService tokenService) : BaseApiController
{
    [Authorize(Roles = "Admin")]
    [HttpPost("register")] // /auth/register
    public async Task<ActionResult<UserAuthDto>> RegisterUser(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("User with name already exist");

        var user = new User
        {
            UserName = registerDto.Username.ToLower(),
            Email = registerDto.Email,
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        await userManager.AddToRoleAsync(user, registerDto.Role);

        if (!result.Succeeded) return BadRequest(result.Errors);

        return new UserAuthDto
        {
            Username = user.UserName,
            Token = await tokenService.CreateToken(user)
        };
    }

    [AllowAnonymous]
    [HttpPost("login")] // /auth/login
    public async Task<ActionResult<UserAuthDto>> LoginUser(LoginDto loginDto)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.NormalizedUserName == loginDto.Username.ToUpper());

        if (user == null || user.UserName == null) return Unauthorized("Invalid username");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized();

        return new UserAuthDto
        {
            Username = user.UserName,
            Token = await tokenService.CreateToken(user)
        }; ;
    }

    // Check if user exist in DB by comparing username in lower case
    // System not register sensitive to case
    // "Test" and "test" are the same
    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}
