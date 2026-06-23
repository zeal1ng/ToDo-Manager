namespace ToDo_Manager.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int UserId { get; set; }
    public User? User { get; set; }
    public List<UserTask> Tasks { get; set; } = new();
}
