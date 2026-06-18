using System.ComponentModel.DataAnnotations;

namespace StudentCourseSystem.Models;

public class Major
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string Code { get; set; } = "";

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
