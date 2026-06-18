using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Models;

namespace StudentCourseSystem.Data;

public class AppDbContext : DbContext
{
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<CourseTimeSlot> CourseTimeSlots => Set<CourseTimeSlot>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Prerequisite> Prerequisites => Set<Prerequisite>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "student_course.db");
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        // 联合唯一：学生+课程+学期（同一学生同一课程同一学期只能一条记录）
        model.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId, e.SemesterId })
            .IsUnique();

        // 联合唯一：课程+时间片
        model.Entity<CourseTimeSlot>()
            .HasIndex(ct => new { ct.CourseId, ct.TimeSlotId })
            .IsUnique();

        // 先修课：防止重复
        model.Entity<Prerequisite>()
            .HasIndex(p => new { p.CourseId, p.PrereqCourseId })
            .IsUnique();

        // 专业课程默认按专业分配
        model.Entity<Course>()
            .HasOne(c => c.Major)
            .WithMany(m => m.Courses)
            .HasForeignKey(c => c.MajorId)
            .OnDelete(DeleteBehavior.SetNull);

        // 先修课关系：Course.Prerequisites -> Prerequisite.Course
        model.Entity<Prerequisite>()
            .HasOne(p => p.Course)
            .WithMany(c => c.Prerequisites)
            .HasForeignKey(p => p.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // 先修课关系：Course.PrerequisiteOf <- Prerequisite.PrereqCourse
        model.Entity<Prerequisite>()
            .HasOne(p => p.PrereqCourse)
            .WithMany(c => c.PrerequisiteOf)
            .HasForeignKey(p => p.PrereqCourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
