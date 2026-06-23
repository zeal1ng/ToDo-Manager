namespace ToDo_Manager.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class CreateCategoryDto
{
    public string Name { get; set; } = null!;
    public int UserId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = null!;
}
