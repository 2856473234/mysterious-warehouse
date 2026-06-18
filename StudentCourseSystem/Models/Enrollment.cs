using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class Enrollment
{
    [Key] public int Id { get; set; }

    public int StudentId { get; set; }
    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    public int SemesterId { get; set; }
    [ForeignKey(nameof(SemesterId))]
    public Semester Semester { get; set; } = null!;

    [Required, MaxLength(10)] public string Status { get; set; } = "已选"; // 已选 / 已退
    public DateTime EnrolledAt { get; set; } = DateTime.Now;
    public DateTime? DroppedAt { get; set; }
    public int? Grade { get; set; } // 百分制成绩，null表示未录入
}
