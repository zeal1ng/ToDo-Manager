using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.SignalR;

namespace ToDo_Manager.Models;

public enum UserRole { Admin, Master }
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    [Required, MaxLength(255), Column("password")]
    public string PasswordHash { get; set; } = null!;
    public UserRole UserRole { get; set; }
    public List<UserTask> Tasks { get; set; } = new List<UserTask>();
}