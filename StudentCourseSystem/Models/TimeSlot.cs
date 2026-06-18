using System.ComponentModel.DataAnnotations;

namespace StudentCourseSystem.Models;

public class TimeSlot
{
    [Key] public int Id { get; set; }
    public int DayOfWeek { get; set; } // 1=周一..5=周五
    public int StartPeriod { get; set; } // 第几节开始
    public int EndPeriod { get; set; }   // 第几节结束
    [MaxLength(50)] public string Label { get; set; } = ""; // "周一1-2节"

    public ICollection<CourseTimeSlot> CourseTimeSlots { get; set; } = new List<CourseTimeSlot>();
}
