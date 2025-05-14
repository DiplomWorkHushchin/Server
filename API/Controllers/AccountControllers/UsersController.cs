using API.Data;
using API.DTOs.AuthDTOs;
using API.DTOs.UserDTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            .Include(u => u.Group)
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

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await userManager.Users
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);

        return Ok(userDto);
    }

    [Authorize]
    [HttpGet("paginations/{start}/{count}")]
    public async Task<ActionResult<List<UserDto>>> GetUsersPaginations(int start, int count)
    {
        var users = await userManager.Users
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Group)
            .Skip(start)
            .Take(count)
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

    // GET api/users/username
    [Authorize]
    //[AllowAnonymous]
    [HttpGet("{username}")]
    public async Task<ActionResult<UserDto>> GetUserByUserName(string username)
    {
        var user = await userManager.Users
            .Include(u => u.Photos)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Group)
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);

        return Ok(userDto);
    }

    // Get api/users/search
    [Authorize]
    [HttpPost("search")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers(UserSearchDto userSearchDto)
    {
        var query = userSearchDto.Query;

        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");

        var parts = query.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var adminRole = await context.Roles
            .Where(r => r.NormalizedName == "ADMIN")
            .FirstOrDefaultAsync();

        if (adminRole == null)
        {
            return NotFound("Admin role not found.");
        }

        var usersQuery = context.Users
            .Where(u =>
                (parts.Length == 1 && (
                    u.FirstName.ToLower().Contains(parts[0]) ||
                    u.LastName.ToLower().Contains(parts[0]) ||
                    u.FatherName.ToLower().Contains(parts[0]))) ||
                (parts.Length == 2 && (
                    u.FirstName.ToLower().Contains(parts[0]) &&
                    u.LastName.ToLower().Contains(parts[1]))) ||
                (parts.Length == 3 && (
                    u.FirstName.ToLower().Contains(parts[0]) &&
                    u.LastName.ToLower().Contains(parts[1]) &&
                    u.FatherName.ToLower().Contains(parts[2])))
            )
            .Where(u => !u.UserRoles.Any(ur => ur.Role.NormalizedName == "ADMIN"))
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FirstName)
            .Include(u => u.Photos)
            .Take(5);

        var users = await usersQuery.ToListAsync();

        if (users == null || !users.Any())
            return NoContent();

        var usersDto = mapper.Map<List<UserDto>>(users);

        return usersDto;
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

    // Patch api/users/update-user
    [Authorize]
    [HttpPatch("update-user")]
    public async Task<ActionResult<UserAuthDto>> UpdateUser(UserUpdateDto userUpdateDto)
    {
        var accessTokenFromHeaders = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(accessTokenFromHeaders))
            return Unauthorized("Cannot find tokens: access");

        var user = await tokenService.GetUserFromTokenAsync(accessTokenFromHeaders);
        if (user == null)
            return NotFound("User not found");

        user.FirstName = userUpdateDto.FirstName;
        user.LastName = userUpdateDto.LastName;

        if (!string.IsNullOrWhiteSpace(userUpdateDto.FatherName))
            user.FatherName = userUpdateDto.FatherName;

        var existingUser = await userManager.FindByNameAsync(userUpdateDto.Username);

        if (existingUser != null && existingUser.Id != user.Id)
        {
            return BadRequest("Username is already taken");
        }
        user.UserName = userUpdateDto.Username;

        if (!string.IsNullOrWhiteSpace(userUpdateDto.DateOfBirth) &&
        DateTime.TryParse(userUpdateDto.DateOfBirth, out var parsedDate))
        {
            user.DateOfBirth = parsedDate.ToUniversalTime().AddDays(1);
        }

        if (!string.IsNullOrWhiteSpace(userUpdateDto.Gender))
            user.Gender = userUpdateDto.Gender;

        if (!string.IsNullOrWhiteSpace(userUpdateDto.PhoneNumber))
            user.PhoneNumber = userUpdateDto.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(userUpdateDto.Country))
            user.Country = userUpdateDto.Country;

        if (!string.IsNullOrWhiteSpace(userUpdateDto.City))
            user.City = userUpdateDto.City;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest("Failed to update user");

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
}
