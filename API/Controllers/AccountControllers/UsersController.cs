using API.Data;
using API.DTOs.UserDTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers.AccountControllers;

[Authorize]
public class UsersController(UserManager<User> userManager, IMapper mapper, DataContext context, 
    IWebHostEnvironment _env, ITokenService tokenService) : BaseApiController
{
    // GET api/users
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();

        if (users == null || !users.Any()) return NotFound("No users found.");

        var usersDto = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = mapper.Map<UserDto>(user);
            var userRoles = await userManager.GetRolesAsync(user);

            var userRole = userRoles.FirstOrDefault();
            if (userRole == null) return BadRequest("User has no roles assigned");

            userDto.UserRoles = userRole;

            usersDto.Add(userDto);
        }
        return usersDto;
    }

    // GET api/users/5
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await userManager.Users
            .Include(u => u.UserPhoto)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var userRole = userRoles.FirstOrDefault();
        if (userRole == null) return BadRequest("User has no roles assigned");

        userDto.UserRoles = userRole;

        return Ok(userDto);
    }

    // Get api/users/username
    [Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUserName(string username)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var userRole = userRoles.FirstOrDefault();
        if (userRole == null) return BadRequest("User has no roles assigned");

        userDto.UserRoles = userRole;

        return Ok(userDto);
    }

    [Authorize]
    [HttpPut("upload-photo")]
    public async Task<ActionResult<UserDto>> UpdateUserPhoto([FromForm] IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest("No file uploaded.");

        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Cannot find tokens: access");

        var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return Unauthorized("Cannot find principal from token");

        var username = principal.Identity?.Name;
        if (username == null || username.Length == 0) return Unauthorized("Username not found in credentials");

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
            return NotFound("User not found");

        var uploadsPath = Path.Combine(_env.WebRootPath, "UploadsUserPictures");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        if (!string.IsNullOrEmpty(user.UserPhoto))
        {
            var oldPhotoPath = Path.Combine(_env.WebRootPath, user.UserPhoto.TrimStart('/'));
            if (System.IO.File.Exists(oldPhotoPath))
            {
                System.IO.File.Delete(oldPhotoPath);
            }
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await photo.CopyToAsync(stream);

        var fileUrl = $"/UploadsUserPictures/{fileName}";

        user.UserPhoto = fileUrl;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest("Failed to update photo");

        var updatedUser = await userManager.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var userDto = mapper.Map<UserDto>(updatedUser);
        var userRoles = await userManager.GetRolesAsync(updatedUser);
        var userRole = userRoles.FirstOrDefault();

        if (userRole != null)
        {
            userDto.UserRoles = userRole;
        }

        return Ok(userDto);
    }


}
