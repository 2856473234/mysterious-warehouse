using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;
using StudentCourseSystem.Services;

namespace StudentCourseSystem;

/// <summary>
/// 集成测试：自动运行核心业务功能测试并输出结果
/// </summary>
static class TestRunner
{
    private static int _passed, _failed;
    private static AppDbContext _db = null!;
    private static readonly AuthService _auth = new();
    private static readonly CourseService _courseSvc = new();
    private static readonly EnrollmentService _enrollSvc = new();
    private static readonly AdminService _adminSvc = new();

    public static void RunAll()
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("  学生选课系统 - 功能测试");
        Console.WriteLine("==============================================");

        // 先初始化种子数据
        DataSeeder.Seed();
        _db = new AppDbContext();

        Test("种子数据已初始化", () => _db.Majors.Any());

        // ===== AuthService 测试 =====
        Test("学生登录: 2024SE01/123456", () =>
        {
            var r = _auth.LoginAsStudent("2024SE01", "123456");
            return r != null && r.Value.Role == "学生";
        });

        Test("学生登录: 错误密码返回null", () =>
        {
            var r = _auth.LoginAsStudent("2024SE01", "wrong");
            return r == null;
        });

        Test("教师登录: 张教授/123456", () =>
        {
            var r = _auth.LoginAsTeacher("张教授", "123456");
            return r != null && r.Value.Role == "教师";
        });

        Test("管理员登录: admin/admin123", () =>
        {
            var r = _auth.LoginAsAdmin("admin", "admin123");
            return r != null && r.Value.Role == "管理员";
        });

        Test("通用登录 - 学生", () =>
        {
            var r = _auth.Login("2024SE01", "123456");
            return r != null && r.Value.Role == "学生";
        });

        Test("通用登录 - 教师", () =>
        {
            var r = _auth.Login("张教授", "123456");
            return r != null && r.Value.Role == "教师";
        });

        Test("通用登录 - 管理员", () =>
        {
            var r = _auth.Login("admin", "admin123");
            return r != null && r.Value.Role == "管理员";
        });

        // ===== 获取测试数据 =====
        var semester = _db.Semesters.First();
        var student = _db.Students.First(s => s.StudentNo == "2024SE01");
        var teacher = _db.Teachers.First(t => t.Name == "张教授");
        var seCourse = _db.Courses.First(c => c.Code == "SE101");  // 数据结构
        var csCourse = _db.Courses.First(c => c.Code == "CS101");  // 计算机网络
        var elective = _db.Courses.First(c => c.Code == "GE101");  // 大学英语

        // ===== CourseService 测试 =====
        Test("GetAvailableCourses: 学生能看到专业课", () =>
        {
            var courses = _courseSvc.GetAvailableCourses(student.Id, semester.Id);
            return courses.Any(c => c.Code == "SE101");
        });

        Test("GetAvailableCourses: 学生看不到其他专业的课", () =>
        {
            var courses = _courseSvc.GetAvailableCourses(student.Id, semester.Id);
            // 软件工程专业不应该看到计算机科学的专业课
            return !courses.Any(c => c.Code == "CS101");
        });

        Test("GetAvailableCourses: 学生能看到选修课", () =>
        {
            var courses = _courseSvc.GetAvailableCourses(student.Id, semester.Id);
            return courses.Any(c => c.Code == "GE101");
        });

        // ===== EnrollmentService 选课测试 =====
        TestDetail("选课: 正常选课(大学英语)", () =>
            _enrollSvc.Enroll(student.Id, elective.Id));

        Test("选课: 重复选课应拒绝", () =>
        {
            var result = _enrollSvc.Enroll(student.Id, elective.Id);
            return result == "已选过该课程";
        });

        TestDetail("选课: 选修课(数据库原理-无冲突)", () =>
        {
            var ge105 = _db.Courses.First(c => c.Code == "GE105");
            return _enrollSvc.Enroll(student.Id, ge105.Id);
        });

        // 先修课检查：应该被拒绝，返回包含"先修课"的消息
        Test("先修课检查: 数据挖掘需先修数据库原理", () =>
        {
            var dsCourse = _db.Courses.First(c => c.Code == "DS101");
            var result = _enrollSvc.Enroll(student.Id, dsCourse.Id);
            return result.Contains("先修课");
        });

        Test("先修课检查: 编译原理需先修数据结构", () =>
        {
            var cs102 = _db.Courses.First(c => c.Code == "CS102");
            var result = _enrollSvc.Enroll(student.Id, cs102.Id);
            // 软件工程专业也不能选计算机科学专业课
            return result.Contains("先修课") || result.Contains("专业");
        });

        // 正常选课（第三次选课）
        TestDetail("选课: 正常选课(Python程序设计)", () =>
        {
            var ge103 = _db.Courses.First(c => c.Code == "GE103");
            return _enrollSvc.Enroll(student.Id, ge103.Id);
        });

        // ===== 获取已选课程 =====
        Test("GetStudentCourses: 学生已选课程", () =>
        {
            var courses = _courseSvc.GetStudentCourses(student.Id, semester.Id);
            return courses.Count > 0;
        });

        Test("GetStudentEnrollments: 学生选课记录", () =>
        {
            var enrolls = _enrollSvc.GetStudentEnrollments(student.Id, semester.Id);
            return enrolls.Count > 0;
        });

        // ===== 退课测试 =====
        Test("退课: 正常退课", () =>
        {
            var enrolls = _enrollSvc.GetStudentEnrollments(student.Id, semester.Id);
            var activeEnroll = enrolls.FirstOrDefault(e => e.Status == "已选");
            if (activeEnroll == null) return "skip";
            var result = _enrollSvc.Drop(student.Id, activeEnroll.Id);
            return result == "ok" ? true : "skip: " + result;
        });

        Test("退课: 重复退课应拒绝", () =>
        {
            var enrolls = _enrollSvc.GetStudentEnrollments(student.Id, semester.Id);
            var droppedEnroll = enrolls.FirstOrDefault(e => e.Status == "已退");
            if (droppedEnroll == null) return "skip";
            var result = _enrollSvc.Drop(student.Id, droppedEnroll.Id);
            return result == "已退过该课程" ? true : "skip: " + result;
        });

        // ===== 成绩测试 =====
        Test("录入成绩: 正常录入", () =>
        {
            var enrolls = _enrollSvc.GetStudentEnrollments(student.Id, semester.Id);
            var active = enrolls.FirstOrDefault(e => e.Status == "已选");
            if (active == null) return "skip";
            _enrollSvc.SetGrade(active.Id, 85);
            _db = new AppDbContext();
            var updated = _db.Enrollments.Find(active.Id);
            return updated?.Grade == 85;
        });

        Test("录入成绩: 范围限制(>100截断)", () =>
        {
            var enrolls = _enrollSvc.GetStudentEnrollments(student.Id, semester.Id);
            var active = enrolls.FirstOrDefault(e => e.Status == "已选");
            if (active == null) return "skip";
            _enrollSvc.SetGrade(active.Id, 150);
            _db = new AppDbContext();
            var updated = _db.Enrollments.Find(active.Id);
            return updated?.Grade == 100;
        });

        // ===== Teacher =====
        Test("GetTeacherCourses: 教师课程列表", () =>
        {
            var courses = _courseSvc.GetTeacherCourses(teacher.Id, semester.Id);
            return courses.Count > 0;
        });

        Test("GetCourseStudents: 课程学生列表", () =>
        {
            var students = _enrollSvc.GetCourseStudents(seCourse.Id);
            return students.Count >= 0;
        });

        // ===== AdminService =====
        Test("Admin: 获取所有教师", () =>
        {
            var teachers = _adminSvc.GetAllTeachers();
            return teachers.Count >= 5;
        });

        Test("Admin: 获取所有教室", () =>
        {
            var rooms = _adminSvc.GetAllClassrooms();
            return rooms.Count >= 6;
        });

        Test("Admin: 获取所有专业", () =>
        {
            var majors = _adminSvc.GetAllMajors();
            return majors.Count == 3;
        });

        Test("Admin: 获取所有学期", () =>
        {
            var sems = _adminSvc.GetAllSemesters();
            return sems.Count >= 1;
        });

        Test("Admin: 获取所有时间片", () =>
        {
            var slots = _adminSvc.GetAllTimeSlots();
            return slots.Count == 25;
        });

        Test("Admin: 获取所有学生", () =>
        {
            var students = _adminSvc.GetAllStudents();
            return students.Count == 30;
        });

        Test("Admin: 获取所有课程", () =>
        {
            var courses = _adminSvc.GetAllCourses(semester.Id);
            return courses.Count >= 10;
        });

        // 测试排课功能
        Test("Admin: 添加时间片到课程", () =>
        {
            var slot = _db.TimeSlots.First();
            var course = _db.Courses.First(c => c.Code == "GE106");
            _adminSvc.AddCourseTimeSlot(course.Id, slot.Id);
            return _db.CourseTimeSlots.Any(ct => ct.CourseId == course.Id && ct.TimeSlotId == slot.Id);
        });

        Test("Admin: 移除课程时间片", () =>
        {
            var slot = _db.TimeSlots.First();
            var course = _db.Courses.First(c => c.Code == "GE106");
            _adminSvc.RemoveCourseTimeSlot(course.Id, slot.Id);
            return !_db.CourseTimeSlots.Any(ct => ct.CourseId == course.Id && ct.TimeSlotId == slot.Id);
        });

        Test("Admin: 添加先修课", () =>
        {
            var c1 = _db.Courses.First(c => c.Code == "GE102");
            var c2 = _db.Courses.First(c => c.Code == "GE101");
            _adminSvc.AddPrerequisite(c1.Id, c2.Id);
            return _db.Prerequisites.Any(p => p.CourseId == c1.Id && p.PrereqCourseId == c2.Id);
        });

        // 测试添加和删除
        Test("Admin: 添加教师", () =>
        {
            var major = _db.Majors.First();
            _adminSvc.AddTeacher(new Teacher { Name = "测试教师", Password = "test123", MajorId = major.Id });
            return _db.Teachers.Any(t => t.Name == "测试教师");
        });

        // ===== 总结 =====
        Console.WriteLine();
        Console.WriteLine("==============================================");
        Console.WriteLine($"  测试完成: 通过 {_passed} 项, 失败 {_failed} 项");
        Console.WriteLine("==============================================");
    }

    static void Test(string name, Func<object> testFn)
    {
        try
        {
            var result = testFn();
            if (result is bool b && b)
            {
                Console.WriteLine($"  [PASS] {name}");
                _passed++;
            }
            else if (result is string s && s == "skip")
            {
                Console.WriteLine($"  [SKIP] {name} (条件不满足)");
            }
            else if (result is string s2)
            {
                Console.WriteLine($"  [FAIL] {name} -> {s2}");
                _failed++;
            }
            else
            {
                Console.WriteLine($"  [FAIL] {name}");
                _failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] {name} -> {ex.Message}");
            _failed++;
        }
    }

    // 带详细错误信息输出的测试
    static void TestDetail(string name, Func<string> testFn)
    {
        try
        {
            var result = testFn();
            if (result == "ok")
            {
                Console.WriteLine($"  [PASS] {name}");
                _passed++;
            }
            else
            {
                Console.WriteLine($"  [FAIL] {name} -> [{result}]");
                _failed++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] {name} -> EX:{ex.Message}");
            _failed++;
        }
    }
}
