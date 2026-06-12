using System.ComponentModel.DataAnnotations;

namespace ToDo_Manager.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
}

public class RegisterUserDto
{
    [Required, MinLength(3), MaxLength(50)]
    public string UserName { get; set; } = null!;

    [Required, MinLength(6)]
    public string Password { get; set; } = null!;
    public int UserRole { get; set; }
}

public class LoginUserDto
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
    public int UserRole { get; set; }
}
