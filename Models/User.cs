using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.SignalR;

namespace ToDo_Manager.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    [Required, MaxLength(255), Column("password")]
    public string PasswordHash { get; set; } = null!;
    public List<Task> Tasks { get; set; } = new List<Task>();
}