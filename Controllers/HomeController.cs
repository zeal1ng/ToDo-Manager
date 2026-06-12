using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_Manager.DTOs;
using ToDo_Manager.Models;
using ToDo_Manager.Services;

namespace ToDo_Manager.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public HomeController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterUserDto model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
        {
            ModelState.AddModelError("", "Username already exists");
            return View(model);
        }
        var user = new User
        {
            UserName = model.UserName,
            PasswordHash = PasswordService.Hash(model.Password),
            UserRole = UserRole.Master
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var token = _tokenService.CreateToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });
        return RedirectToAction("Index");
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginUserDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
        if (user == null || !PasswordService.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }
        var token = _tokenService.CreateToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("jwt");
        return RedirectToAction("Index");
    }

}
