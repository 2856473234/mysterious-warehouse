using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class Course
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [Required, MaxLength(20)] public string Code { get; set; } = "";
    public int Credits { get; set; }
    [Required, MaxLength(10)] public string Type { get; set; } = "选修"; // 专业 / 选修

    public int TeacherId { get; set; }
    [ForeignKey(nameof(TeacherId))]
    public Teacher Teacher { get; set; } = null!;

    public int ClassroomId { get; set; }
    [ForeignKey(nameof(ClassroomId))]
    public Classroom Classroom { get; set; } = null!;

    public int MaxStudents { get; set; }

    public int? MajorId { get; set; } // 专业课关联专业，选修课为null
    [ForeignKey(nameof(MajorId))]
    public Major? Major { get; set; }

    public int SemesterId { get; set; }
    [ForeignKey(nameof(SemesterId))]
    public Semester Semester { get; set; } = null!;

    public ICollection<CourseTimeSlot> CourseTimeSlots { get; set; } = new List<CourseTimeSlot>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Prerequisite> PrerequisiteOf { get; set; } = new List<Prerequisite>(); // 作为先修课
    public ICollection<Prerequisite> Prerequisites { get; set; } = new List<Prerequisite>();  // 需要先修课
}
