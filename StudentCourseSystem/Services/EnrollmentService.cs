using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;

namespace StudentCourseSystem.Services;

public class EnrollmentService
{
    private const int MaxCoursesPerStudent = 5;

    public string Enroll(int studentId, int courseId)
    {
        using var db = new AppDbContext();

        var course = db.Courses
            .Include(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Include(c => c.Semester)
            .FirstOrDefault(c => c.Id == courseId);
        if (course == null) return "课程不存在";

        // 1. 是否在选课窗口内
        var now = DateTime.Now;
        if (now < course.Semester.EnrollStart || now > course.Semester.EnrollEnd)
            return "当前不在选课窗口期内";

        // 2. 是否已选过且未退课
        var existing = db.Enrollments
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId && e.SemesterId == course.SemesterId);
        if (existing != null)
            return existing.Status == "已选" ? "已选过该课程" : "已退过该课程，不能再次选择";

        // 3. 本学期已选课程数检查（最多5节）
        var enrolledCount = db.Enrollments
            .Count(e => e.StudentId == studentId && e.SemesterId == course.SemesterId && e.Status == "已选");
        if (enrolledCount >= MaxCoursesPerStudent)
            return $"每学期最多选{MaxCoursesPerStudent}门课，已达到上限";

        // 4. 先修课检查
        var prereqs = db.Prerequisites.Where(p => p.CourseId == courseId).ToList();
        foreach (var prereq in prereqs)
        {
            var passed = db.Enrollments.Any(e =>
                e.StudentId == studentId &&
                e.CourseId == prereq.PrereqCourseId &&
                e.Status == "已选" &&
                e.Grade != null && e.Grade >= 60);
            if (!passed)
            {
                var prereqName = db.Courses.Where(c => c.Id == prereq.PrereqCourseId).Select(c => c.Name).FirstOrDefault();
                return $"未通过先修课「{prereqName}」";
            }
        }

        // 5. 名额检查
        var currentEnrolled = db.Enrollments
            .Count(e => e.CourseId == courseId && e.SemesterId == course.SemesterId && e.Status == "已选");
        if (currentEnrolled >= course.MaxStudents)
            return "该课程已满";

        // 6. 时间冲突检查（学生的时间）
        var studentTimeSlotIds = db.Enrollments
            .Where(e => e.StudentId == studentId && e.SemesterId == course.SemesterId && e.Status == "已选")
            .Join(db.CourseTimeSlots, e => e.CourseId, ct => ct.CourseId, (e, ct) => ct.TimeSlotId)
            .ToHashSet();

        foreach (var ct in course.CourseTimeSlots)
        {
            if (studentTimeSlotIds.Contains(ct.TimeSlotId))
            {
                var ts = db.TimeSlots.Find(ct.TimeSlotId);
                return $"时间冲突：{ts?.Label}";
            }
        }

        // 7. 教师冲突检查
        var teacherCourseIds = db.Courses
            .Where(c => c.TeacherId == course.TeacherId && c.SemesterId == course.SemesterId && c.Id != courseId)
            .Select(c => c.Id)
            .ToHashSet();
        var teacherTimeSlotIds = db.CourseTimeSlots
            .Where(ct => teacherCourseIds.Contains(ct.CourseId))
            .Select(ct => ct.TimeSlotId)
            .ToHashSet();
        foreach (var ct in course.CourseTimeSlots)
        {
            if (teacherTimeSlotIds.Contains(ct.TimeSlotId))
            {
                var ts = db.TimeSlots.Find(ct.TimeSlotId);
                return $"教师该时间段已有课：{ts?.Label}";
            }
        }

        // ✅ 选课成功
        db.Enrollments.Add(new Models.Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            SemesterId = course.SemesterId,
            Status = "已选",
            EnrolledAt = now
        });
        db.SaveChanges();
        return "ok";
    }

    public string Drop(int studentId, int enrollmentId)
    {
        using var db = new AppDbContext();
        var enrollment = db.Enrollments
            .Include(e => e.Course)
            .FirstOrDefault(e => e.Id == enrollmentId && e.StudentId == studentId);
        if (enrollment == null) return "选课记录不存在";
        if (enrollment.Status == "已退") return "已退过该课程";

        enrollment.Status = "已退";
        enrollment.DroppedAt = DateTime.Now;
        db.SaveChanges();
        return "ok";
    }

    public void SetGrade(int enrollmentId, int grade)
    {
        using var db = new AppDbContext();
        var enrollment = db.Enrollments.Find(enrollmentId);
        if (enrollment != null)
        {
            enrollment.Grade = Math.Clamp(grade, 0, 100);
            db.SaveChanges();
        }
    }

    public List<Models.Enrollment> GetStudentEnrollments(int studentId, int semesterId)
    {
        using var db = new AppDbContext();
        return db.Enrollments
            .Include(e => e.Course).ThenInclude(c => c.Teacher)
            .Include(e => e.Course).ThenInclude(c => c.CourseTimeSlots).ThenInclude(ct => ct.TimeSlot)
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId)
            .OrderBy(e => e.Status)
            .ThenBy(e => e.EnrolledAt)
            .ToList();
    }

    public List<Models.Enrollment> GetCourseStudents(int courseId)
    {
        using var db = new AppDbContext();
        return db.Enrollments
            .Include(e => e.Student).ThenInclude(s => s.Major)
            .Where(e => e.CourseId == courseId && e.Status == "已选")
            .OrderBy(e => e.Student.StudentNo)
            .ToList();
    }
}
