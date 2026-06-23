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
public class UserTaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserTaskController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<List<UserTaskDto>>> GetTasks([FromQuery] int? categoryId)
    {
        var userId = GetUserId();
        return await GetTasksForUser(userId, categoryId);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<UserTaskDto>>> GetUserTasks(int userId, [FromQuery] int? categoryId)
    {
        return await GetTasksForUser(userId, categoryId);
    }

    private async Task<ActionResult<List<UserTaskDto>>> GetTasksForUser(int userId, int? categoryId = null)
    {
        var query = _context.UserTasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new UserTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Body = t.Body,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt
            })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost("create")]
    public async Task<ActionResult<UserTaskDto>> AddUserTask([FromBody] CreateUserTaskDto dto)
    {
        var userId = GetUserId();
        if (IsAdmin() && dto.UserId > 0)
            userId = dto.UserId;

        var task = new UserTask
        {
            Title = dto.Title,
            Body = dto.Body ?? string.Empty,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = Status.Incompleted,
            Priority = dto.Priority == "Urgent" ? Priority.Urgent : Priority.Normal,
            CategoryId = dto.CategoryId
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTasks), null, new UserTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Body = task.Body,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            CategoryId = task.CategoryId,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateUserTaskDto dto)
    {
        var task = await _context.UserTasks.FindAsync(id);
        if (task == null) return NotFound();
        if (!IsAdmin() && task.UserId != GetUserId())
            return Forbid();

        if (dto.Title != null) task.Title = dto.Title;
        if (dto.Body != null) task.Body = dto.Body;
        if (dto.Status != null)
        {
            task.Status = dto.Status == "Completed" ? Status.Completed : Status.Incompleted;
            task.CompletedAt = task.Status == Status.Completed ? DateTime.UtcNow : null;
        }
        if (dto.Priority != null)
            task.Priority = dto.Priority == "Urgent" ? Priority.Urgent : Priority.Normal;
        if (dto.CategoryId.HasValue)
            task.CategoryId = dto.CategoryId.Value > 0 ? dto.CategoryId : null;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var task = await _context.UserTasks.FindAsync(id);
        if (task == null) return NotFound();
        if (!IsAdmin() && task.UserId != GetUserId())
            return Forbid();

        _context.UserTasks.Remove(task);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats([FromQuery] int days = 7, [FromQuery] int? userId = null)
    {
        var uid = IsAdmin() && userId.HasValue ? userId.Value : GetUserId();
        var since = DateTime.UtcNow.Date.AddDays(-days + 1);

        var created = await _context.UserTasks
            .Where(t => t.UserId == uid && t.CreatedAt >= since)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var completed = await _context.UserTasks
            .Where(t => t.UserId == uid && t.CompletedAt != null && t.CompletedAt >= since)
            .GroupBy(t => t.CompletedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new List<object>();
        for (var date = since; date <= DateTime.UtcNow.Date; date = date.AddDays(1))
        {
            result.Add(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Created = created.FirstOrDefault(c => c.Date == date)?.Count ?? 0,
                Completed = completed.FirstOrDefault(c => c.Date == date)?.Count ?? 0
            });
        }

        return Ok(result);
    }

    [HttpDelete("clear-completed")]
    public async Task<ActionResult> ClearCompleted([FromQuery] int? userId)
    {
        var uid = IsAdmin() && userId.HasValue ? userId.Value : GetUserId();

        var completed = await _context.UserTasks
            .Where(t => t.UserId == uid && t.Status == Status.Completed)
            .ToListAsync();

        _context.UserTasks.RemoveRange(completed);
        await _context.SaveChangesAsync();
        return Ok();
    }
}