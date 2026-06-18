using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;

namespace StudentCourseSystem.Services;

public class AdminService
{
    public List<Teacher> GetAllTeachers()
    {
        using var db = new AppDbContext();
        return db.Teachers.Include(t => t.Major).ToList();
    }
    public void AddTeacher(Teacher teacher)
    {
        using var db = new AppDbContext();
        db.Teachers.Add(teacher);
        db.SaveChanges();
    }
    public void DeleteTeacher(int id)
    {
        using var db = new AppDbContext();
        var t = db.Teachers.Find(id);
        if (t != null) { db.Teachers.Remove(t); db.SaveChanges(); }
    }

    public List<Course> GetAllCourses(int semesterId)
    {
        using var db = new AppDbContext();
        return db.Courses
            .Include(c => c.Teacher)
            .Include(c => c.Classroom)
            .Include(c => c.Major)
            .Include(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Where(c => c.SemesterId == semesterId)
            .ToList();
    }
    public void AddCourse(Course course)
    {
        using var db = new AppDbContext();
        db.Courses.Add(course);
        db.SaveChanges();
    }
    public void DeleteCourse(int id)
    {
        using var db = new AppDbContext();
        using var tx = db.Database.BeginTransaction();
        var c = db.Courses.Include(x => x.CourseTimeSlots).FirstOrDefault(x => x.Id == id);
        if (c != null)
        {
            db.CourseTimeSlots.RemoveRange(c.CourseTimeSlots);
            db.Courses.Remove(c);
            db.SaveChanges();
            tx.Commit();
        }
    }

    public List<Classroom> GetAllClassrooms()
    {
        using var db = new AppDbContext();
        return db.Classrooms.ToList();
    }
    public void AddClassroom(Classroom room)
    {
        using var db = new AppDbContext();
        db.Classrooms.Add(room);
        db.SaveChanges();
    }

    public void DeleteClassroom(int id)
    {
        using var db = new AppDbContext();
        var room = db.Classrooms.Find(id);
        if (room != null) { db.Classrooms.Remove(room); db.SaveChanges(); }
    }

    public List<Major> GetAllMajors()
    {
        using var db = new AppDbContext();
        return db.Majors.ToList();
    }

    public List<Semester> GetAllSemesters()
    {
        using var db = new AppDbContext();
        return db.Semesters.OrderByDescending(s => s.StartDate).ToList();
    }
    public void AddSemester(Semester semester)
    {
        using var db = new AppDbContext();
        db.Semesters.Add(semester);
        db.SaveChanges();
    }

    public void AddCourseTimeSlot(int courseId, int timeSlotId)
    {
        using var db = new AppDbContext();
        if (!db.CourseTimeSlots.Any(ct => ct.CourseId == courseId && ct.TimeSlotId == timeSlotId))
        {
            db.CourseTimeSlots.Add(new CourseTimeSlot { CourseId = courseId, TimeSlotId = timeSlotId });
            db.SaveChanges();
        }
    }
    public void RemoveCourseTimeSlot(int courseId, int timeSlotId)
    {
        using var db = new AppDbContext();
        var ct = db.CourseTimeSlots.FirstOrDefault(x => x.CourseId == courseId && x.TimeSlotId == timeSlotId);
        if (ct != null) { db.CourseTimeSlots.Remove(ct); db.SaveChanges(); }
    }

    public List<TimeSlot> GetAllTimeSlots()
    {
        using var db = new AppDbContext();
        return db.TimeSlots.OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartPeriod).ToList();
    }

    public void AddPrerequisite(int courseId, int prereqCourseId)
    {
        using var db = new AppDbContext();
        if (!db.Prerequisites.Any(p => p.CourseId == courseId && p.PrereqCourseId == prereqCourseId))
        {
            db.Prerequisites.Add(new Prerequisite { CourseId = courseId, PrereqCourseId = prereqCourseId });
            db.SaveChanges();
        }
    }

    public List<Student> GetAllStudents()
    {
        using var db = new AppDbContext();
        return db.Students.Include(s => s.Major).ToList();
    }
}
