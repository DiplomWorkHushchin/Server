using API.Data;
using API.DTOs.AuthDTOs;
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
public class UsersController(
    UserManager<User> userManager, 
    IMapper mapper, 
    ITokenService tokenService,
    IFileService fileService,
    DataContext context
    ) : BaseApiController
{
   // GET api/users
   [Authorize(Roles = "Admin")]
   [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await userManager.Users
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .ToListAsync();

        if (users == null || !users.Any()) return NotFound("No users found.");

        var usersDto = new List<UserDto>();

        foreach (var user in users)
        {
            var userDto = mapper.Map<UserDto>(user);
         
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
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);
       
        return Ok(userDto);
    }

    // GET api/users/username
    [Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUserName(string username)
    {
        var user = await userManager.Users
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);

        return Ok(userDto);
    }


    // PUT api/users/upload-photo
    [Authorize]
    [HttpPut("upload-photo")]
    public async Task<ActionResult<UserDto>> UpdateUserPhoto([FromForm] IFormFile photo)
    {
        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Cannot find tokens: access");

        if (photo == null || photo.Length == 0)
            return BadRequest("No file uploaded.");

        var user = await tokenService.GetUserFromTokenAsync(accessToken);

        if (user == null)
            return NotFound("User not found");

        var userPhotos = user.Photos.ToList();

        if (userPhotos.Any())
        {
            foreach (var userPhoto in userPhotos)
            {
                fileService.DeleteFile(userPhoto.Url); 
                context.UserPhotos.Remove(userPhoto);
            }

            await context.SaveChangesAsync();
        }

        var filePath = await fileService.SaveFileAsync(photo, "UploadsUserPictures");

        user.Photos.Add(new UserPhoto
        {
            Url = filePath,
            Created = DateTime.UtcNow,
            UserId = user.Id
        });

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest("Failed to update photo");

        var userDto = mapper.Map<UserDto>(user);

        return Ok(userDto);
    }
}
