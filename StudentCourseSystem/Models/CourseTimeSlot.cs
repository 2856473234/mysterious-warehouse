using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentCourseSystem.Models;

public class CourseTimeSlot
{
    [Key] public int Id { get; set; }

    public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    public int TimeSlotId { get; set; }
    [ForeignKey(nameof(TimeSlotId))]
    public TimeSlot TimeSlot { get; set; } = null!;
}
