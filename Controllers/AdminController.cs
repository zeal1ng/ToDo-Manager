using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_Manager.Models;
using ToDo_Manager.Services;

namespace ToDo_Manager.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<ActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.UserRole,
                TotalTasks = u.Tasks.Count,
                CompletedTasks = u.Tasks.Count(t => t.Status == Status.Completed)
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == currentUserId)
            return BadRequest(new { message = "Cannot delete yourself" });

        var user = await _context.Users
            .Include(u => u.Tasks)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        _context.UserTasks.RemoveRange(user.Tasks);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("users")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserAdminDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
            return BadRequest(new { message = "Username already exists" });

        var user = new User
        {
            UserName = dto.UserName,
            PasswordHash = PasswordService.Hash(dto.Password),
            UserRole = dto.UserRole
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { user.Id, user.UserName, user.UserRole });
    }
}

public class CreateUserAdminDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole UserRole { get; set; }
}
