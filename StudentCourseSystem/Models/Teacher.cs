using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class Teacher
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(100)] public string Password { get; set; } = "";

    public int MajorId { get; set; }
    [ForeignKey(nameof(MajorId))]
    public Major Major { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
