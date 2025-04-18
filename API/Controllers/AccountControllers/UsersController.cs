using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AccountControllers;

[Authorize]
public class UsersController(DataContext context) : BaseApiController
{
    // Database context
    private readonly DataContext _context = context;

    // GET api/users
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        if (users == null || !users.Any()) return NotFound("No users found.");

        return users;
    }

    // GET api/users/5
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null) return NotFound("User not found");

        return Ok(user);
    }
}
