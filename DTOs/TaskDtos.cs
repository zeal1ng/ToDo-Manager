namespace ToDo_Manager.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Status { get; set; }
}
