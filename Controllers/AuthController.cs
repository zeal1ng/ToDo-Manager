using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_Manager.Models;
using ToDo_Manager.Services;
using ToDo_Manager.DTOs;

namespace ToDo_Manager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == request.Name))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        var user = new User
        {
            UserName = request.Name,
            PasswordHash = PasswordService.Hash(request.Password),
            UserRole = UserRole.Master
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);

        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.Name);
        if (user == null || !PasswordService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new { token });
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        Response.Cookies.Delete("jwt");
        return Ok(new { message = "Logged out successfully" });
    }
}