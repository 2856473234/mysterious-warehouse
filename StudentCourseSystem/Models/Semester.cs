using System.ComponentModel.DataAnnotations;

namespace StudentCourseSystem.Models;

public class Semester
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int WeekCount { get; set; } = 16;
    public DateTime EnrollStart { get; set; }
    public DateTime EnrollEnd { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
