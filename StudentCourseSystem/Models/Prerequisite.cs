using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class Prerequisite
{
    [Key] public int Id { get; set; }

    public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    public int PrereqCourseId { get; set; }
    [ForeignKey(nameof(PrereqCourseId))]
    public Course PrereqCourse { get; set; } = null!;
}
