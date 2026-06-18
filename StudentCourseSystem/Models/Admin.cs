using System.ComponentModel.DataAnnotations;

namespace StudentCourseSystem.Models;

public class Admin
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Username { get; set; } = "";
    [Required, MaxLength(100)] public string Password { get; set; } = "";
}
