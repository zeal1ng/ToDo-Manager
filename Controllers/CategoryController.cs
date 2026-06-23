using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_Manager.Models;
using ToDo_Manager.DTOs;

namespace ToDo_Manager.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] int? userId)
    {
        var uid = IsAdmin() && userId.HasValue ? userId.Value : GetUserId();
        var categories = await _context.Categories
            .Where(c => c.UserId == uid)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var userId = GetUserId();
        if (IsAdmin() && dto.UserId > 0)
            userId = dto.UserId;

        var category = new Category
        {
            Name = dto.Name,
            UserId = userId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), null, new CategoryDto
        {
            Id = category.Id,
            Name = category.Name
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        if (!IsAdmin() && category.UserId != GetUserId())
            return Forbid();

        category.Name = dto.Name;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        if (!IsAdmin() && category.UserId != GetUserId())
            return Forbid();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
