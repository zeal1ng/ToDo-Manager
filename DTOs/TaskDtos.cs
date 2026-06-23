namespace ToDo_Manager.DTOs;

public class UserTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority {get; set; } = null!;
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateUserTaskDto
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public int UserId { get; set; }
    public string? Priority { get; set; }
    public int? CategoryId { get; set; }
}

public class GetUserTaskDto
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class UpdateUserTaskDto
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? CategoryId { get; set; }
}
