using API.Data;
using API.DTOs.UserDTOs;
using API.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AccountControllers;

[Authorize]
public class UsersController(UserManager<User> userManager, IMapper mapper) : BaseApiController
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
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user == null) return NotFound("User not found");

        var userDto = mapper.Map<UserDto>(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var userRole = userRoles.FirstOrDefault();
        if (userRole == null) return BadRequest("User has no roles assigned");

        userDto.UserRoles = userRole;

        return Ok(userDto);
    }
}
