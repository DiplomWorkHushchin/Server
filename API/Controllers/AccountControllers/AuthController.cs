using API.Data;
using API.DTOs.AuthDTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers.AccountControllers;

public class AuthController(DataContext context, ITokenService tokenService) : BaseApiController
{
    private readonly DataContext _context = context;

    [HttpPost("register")] // /auth/register
    public async Task<ActionResult<UserAuthDto>> RegisterUser(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("User with name already exist");
        
        using var hmac = new HMACSHA512();

        var user = new User
        {
            UserName = registerDto.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserAuthDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")] // /auth/login
    public async Task<ActionResult<UserAuthDto>> LoginUser(LoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        return new UserAuthDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        }; ;
    }

    // Check if user exist in DB by comparing username in lower case
    // System not register sensitive to case
    // "Test" and "test" are the same
    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
}
