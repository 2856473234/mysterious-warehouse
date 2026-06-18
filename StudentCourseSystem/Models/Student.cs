using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class Student
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string StudentNo { get; set; } = "";
    [Required, MaxLength(100)] public string Password { get; set; } = "";
    public int Grade { get; set; }

    public int MajorId { get; set; }
    [ForeignKey(nameof(MajorId))]
    public Major Major { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
