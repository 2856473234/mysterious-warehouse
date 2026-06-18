using System.ComponentModel.DataAnnotations;

namespace StudentCourseSystem.Models;

public class Classroom
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    public int Capacity { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
