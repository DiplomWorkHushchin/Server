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

public class AuthController(UserManager<User> userManager, ITokenService tokenService, IMapper mapper) : BaseApiController
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
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        await userManager.AddToRoleAsync(user, registerDto.Role);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        var UserDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);
        UserDto.UserRoles = userRoles.FirstOrDefault();

        return new UserAuthDto
        {
            User = UserDto,
            Token = accessToken
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

        var (accessToken, refreshToken) = await tokenService.CreateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        var UserDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);
        UserDto.UserRoles = userRoles.FirstOrDefault();

        return new UserAuthDto
        {
            User = UserDto,
            Token = accessToken
        };
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")] // /auth/refresh-token
    public async Task<ActionResult<UserAuthDto>> UpdateUserTokens()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken))
            return Unauthorized("Cannot find tokens");

        var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return Unauthorized("Cannot find principal from token");

        var username = principal.Identity?.Name;
        if (username == null || username.Length == 0) return Unauthorized("Username not found in credentials");

        var user = await userManager.FindByNameAsync(username);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized("Invalid refresh token");

        var (newAccessToken, newRefreshToken) = await tokenService.CreateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", newRefreshToken, cookieOptions);

        var UserDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);
        UserDto.UserRoles = userRoles.FirstOrDefault();

        return new UserAuthDto
        {
            User = UserDto,
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
