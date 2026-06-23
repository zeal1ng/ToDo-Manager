namespace ToDo_Manager.Models;

public enum Status { Completed, Incompleted }
public enum Priority { Urgent, Normal }
public class UserTask
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public int UserId { get; set; }
    public User? User { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public Status Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}