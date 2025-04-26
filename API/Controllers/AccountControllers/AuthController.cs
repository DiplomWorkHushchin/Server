using API.DTOs.AuthDTOs;
using API.DTOs.UserDTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AccountControllers;

public class AuthController(
    UserManager<User> userManager, 
    ITokenService tokenService, 
    IMapper mapper
    ) : BaseApiController
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
            FatherName = "",
            FirstName = "",
            LastName = "",
            PhoneNumber = "",
            UserRoles = new List<UserRole>
            {
                new UserRole
                {
                    Role = new Role
                    {
                        Name = registerDto.Role
                    }
                }
            },
            Photos = new List<UserPhoto>()
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        });

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = accessToken
        };
    }

    [AllowAnonymous]
    [HttpPost("login")] // /auth/login
    public async Task<ActionResult<UserAuthDto>> LoginUser(LoginDto loginDto)
    {
        var user = await userManager.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Photos)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == loginDto.Email.ToUpper());
        if (user == null || user.UserName == null) return Unauthorized("Invalid username");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) return Unauthorized("Check you login data");

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = accessToken,
        };
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")] // /auth/refresh-token
    public async Task<ActionResult<UserAuthDto>> UpdateUserTokens()
    {
        // Get refresh token from cookie, check for null
        var refreshToken = Request.Cookies["refreshToken"];
        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        Console.WriteLine($"Refresh token: {refreshToken}");
        Console.WriteLine($"Access token: {accessToken}");

        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Cannot find tokens: access");

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Cannot find tokens: refresh");

        var user = await tokenService.GetUserFromTokenAsync(accessToken);

        if (user == null) return Unauthorized("No users founded");

        var oldRefreshToken = user.RefreshTokens.FirstOrDefault();
        Console.WriteLine($"Old refresh token: {oldRefreshToken?.Token}");

        if (oldRefreshToken == null || oldRefreshToken.ExpiryDate < DateTime.UtcNow)
            return Unauthorized("No refresh token in db founded");

        var (newAccessToken, newRefreshToken) = await tokenService.CreateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        var userDto = mapper.Map<UserDto>(user);

        return new UserAuthDto
        {
            User = userDto,
            Token = newAccessToken
        };
    }

    // Check if user exist in DB by comparing username in NORMILIZED view
    // System not register sensitive to case
    // "Test" and "test" are the same
    private async Task<bool> UserExists(string username)
    {
        return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
    }
}
