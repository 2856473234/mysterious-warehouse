using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;

namespace StudentCourseSystem.Services;

public class CourseService
{
    public List<Course> GetAvailableCourses(int studentId, int semesterId)
    {
        using var db = new AppDbContext();
        var student = db.Students.Find(studentId);
        if (student == null) return new();

        var enrolledCourseIds = db.Enrollments
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId && e.Status == "已选")
            .Select(e => e.CourseId)
            .ToHashSet();

        return db.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Classroom)
            .Include(c => c.Major)
            .Include(c => c.Semester)
            .Include(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Where(c => c.SemesterId == semesterId)
            .AsEnumerable()
            .Where(c => !enrolledCourseIds.Contains(c.Id)) // 还没选
            .Where(c =>
            {
                // 专业课：只能选自己专业的
                if (c.Type == "专业") return c.MajorId == student.MajorId;
                return true; // 选修课不限
            })
            .ToList();
    }

    public List<Course> GetStudentCourses(int studentId, int semesterId)
    {
        using var db = new AppDbContext();
        return db.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Classroom)
            .Include(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Where(c => c.Enrollments.Any(e => e.StudentId == studentId && e.SemesterId == semesterId && e.Status == "已选"))
            .ToList();
    }

    public List<Course> GetTeacherCourses(int teacherId, int semesterId)
    {
        using var db = new AppDbContext();
        return db.Courses
            .Include(c => c.Classroom)
            .Include(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Where(c => c.TeacherId == teacherId && c.SemesterId == semesterId)
            .ToList();
    }
}
