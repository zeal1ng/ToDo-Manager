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
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


}
