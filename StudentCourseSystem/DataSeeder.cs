using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;

namespace StudentCourseSystem;

public static class DataSeeder
{
    public static void Seed()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();

        if (db.Majors.Any()) return; // 已有数据

        // ===== 学院与专业 =====
        var majors = new List<Major>
        {
            new() { Name = "软件工程", Code = "SE" },
            new() { Name = "计算机科学", Code = "CS" },
            new() { Name = "大数据", Code = "DS" },
        };
        db.Majors.AddRange(majors);
        db.SaveChanges();

        // ===== 管理员 =====
        db.Admins.Add(new Admin { Username = "admin", Password = "admin123" });

        // ===== 教师 =====
        var teachers = new List<Teacher>
        {
            new() { Name = "张教授", Password = "123456", MajorId = majors[0].Id },
            new() { Name = "李教授", Password = "123456", MajorId = majors[0].Id },
            new() { Name = "王教授", Password = "123456", MajorId = majors[1].Id },
            new() { Name = "赵教授", Password = "123456", MajorId = majors[1].Id },
            new() { Name = "陈教授", Password = "123456", MajorId = majors[2].Id },
        };
        db.Teachers.AddRange(teachers);

        // ===== 学生 (每专业10人，共30人) =====
        var students = new List<Student>();
        var names = new[] { "小明", "小红", "小刚", "小丽", "小华", "小美", "小强", "小芳", "小龙", "小雪" };
        foreach (var major in majors)
        {
            for (int i = 0; i < 10; i++)
            {
                var studentNo = $"2024{major.Code}{i + 1:D2}";
                students.Add(new Student
                {
                    Name = names[i],
                    StudentNo = studentNo,
                    Password = "123456",
                    Grade = 2024,
                    MajorId = major.Id
                });
            }
        }
        db.Students.AddRange(students);

        // ===== 教室 =====
        var classrooms = new List<Classroom>
        {
            new() { Name = "教301", Capacity = 60 },
            new() { Name = "教302", Capacity = 50 },
            new() { Name = "教303", Capacity = 80 },
            new() { Name = "教401", Capacity = 40 },
            new() { Name = "教402", Capacity = 100 },
            new() { Name = "计院机房1", Capacity = 60 },
        };
        db.Classrooms.AddRange(classrooms);
        db.SaveChanges();

        // ===== 学期 =====
        var semester = new Semester
        {
            Name = "2025-2026第二学期",
            StartDate = new DateTime(2026, 2, 1),
            EndDate = new DateTime(2026, 7, 1),
            WeekCount = 16,
            EnrollStart = new DateTime(2026, 1, 1),
            EnrollEnd = new DateTime(2026, 12, 31)
        };
        db.Semesters.Add(semester);
        db.SaveChanges();

        // ===== 时间片 (周一~周五, 每天5个时段) =====
        var days = new[] { "周一", "周二", "周三", "周四", "周五" };
        var periods = new[] { (1, 2, "1-2节"), (3, 4, "3-4节"), (5, 6, "5-6节"), (7, 8, "7-8节"), (9, 10, "9-10节") };
        var slots = new List<TimeSlot>();
        int dayNum = 1;
        foreach (var day in days)
        {
            foreach (var (sp, ep, label) in periods)
                slots.Add(new TimeSlot { DayOfWeek = dayNum, StartPeriod = sp, EndPeriod = ep, Label = $"{day}{label}" });
            dayNum++;
        }
        db.TimeSlots.AddRange(slots);
        db.SaveChanges();

        // ===== 课程 =====
        // 专业课：软件工程
        var seCourse1 = new Course { Name = "数据结构", Code = "SE101", Credits = 4, Type = "专业", TeacherId = teachers[0].Id, ClassroomId = classrooms[0].Id, MaxStudents = 60, MajorId = majors[0].Id, SemesterId = semester.Id };
        var seCourse2 = new Course { Name = "操作系统", Code = "SE102", Credits = 3, Type = "专业", TeacherId = teachers[1].Id, ClassroomId = classrooms[1].Id, MaxStudents = 50, MajorId = majors[0].Id, SemesterId = semester.Id };
        // 专业课前5周每周多节: 在排课管理里手动配
        // 专业课：计算机科学
        var csCourse1 = new Course { Name = "计算机网络", Code = "CS101", Credits = 3, Type = "专业", TeacherId = teachers[2].Id, ClassroomId = classrooms[2].Id, MaxStudents = 80, MajorId = majors[1].Id, SemesterId = semester.Id };
        var csCourse2 = new Course { Name = "编译原理", Code = "CS102", Credits = 4, Type = "专业", TeacherId = teachers[3].Id, ClassroomId = classrooms[3].Id, MaxStudents = 40, MajorId = majors[1].Id, SemesterId = semester.Id };
        // 专业课：大数据
        var dsCourse1 = new Course { Name = "数据挖掘", Code = "DS101", Credits = 3, Type = "专业", TeacherId = teachers[4].Id, ClassroomId = classrooms[4].Id, MaxStudents = 60, MajorId = majors[2].Id, SemesterId = semester.Id };
        // 选修课 (全校)
        var electives = new List<Course>
        {
            new() { Name = "大学英语", Code = "GE101", Credits = 2, Type = "选修", TeacherId = teachers[0].Id, ClassroomId = classrooms[0].Id, MaxStudents = 60, SemesterId = semester.Id },
            new() { Name = "高等数学", Code = "GE102", Credits = 3, Type = "选修", TeacherId = teachers[1].Id, ClassroomId = classrooms[4].Id, MaxStudents = 80, SemesterId = semester.Id },
            new() { Name = "Python程序设计", Code = "GE103", Credits = 3, Type = "选修", TeacherId = teachers[2].Id, ClassroomId = classrooms[5].Id, MaxStudents = 60, SemesterId = semester.Id },
            new() { Name = "人工智能导论", Code = "GE104", Credits = 2, Type = "选修", TeacherId = teachers[3].Id, ClassroomId = classrooms[2].Id, MaxStudents = 80, SemesterId = semester.Id },
            new() { Name = "数据库原理", Code = "GE105", Credits = 3, Type = "选修", TeacherId = teachers[4].Id, ClassroomId = classrooms[4].Id, MaxStudents = 60, SemesterId = semester.Id },
            new() { Name = "软件工程导论", Code = "GE106", Credits = 2, Type = "选修", TeacherId = teachers[0].Id, ClassroomId = classrooms[1].Id, MaxStudents = 50, SemesterId = semester.Id },
        };
        db.Courses.AddRange(new[] { seCourse1, seCourse2, csCourse1, csCourse2, dsCourse1 });
        db.Courses.AddRange(electives);
        db.SaveChanges();

        // ===== 课程时间片分配 =====
        // slot index: 周一1-2=1, 周一3-4=2, 周一5-6=3, 周一7-8=4, 周一9-10=5
        //            周二1-2=6, 周二3-4=7, 周二5-6=8, 周二7-8=9, 周二9-10=10
        //            周三1-2=11, 周三3-4=12, 周三5-6=13, 周三7-8=14, 周三9-10=15
        //            周四1-2=16, 周四3-4=17, 周四5-6=18, 周四7-8=19, 周四9-10=20
        //            周五1-2=21, 周五3-4=22, 周五5-6=23, 周五7-8=24, 周五9-10=25
        void AssignTime(int courseId, int slotId)
        {
            db.CourseTimeSlots.Add(new CourseTimeSlot { CourseId = courseId, TimeSlotId = slotId });
        }

        // 专业课 (各自占用不同时段)
        // 数据结构: 周一1-2节 + 周三3-4节 (专业课每周多节)
        AssignTime(seCourse1.Id, 1); AssignTime(seCourse1.Id, 12);
        // 操作系统: 周二1-2节 + 周四3-4节
        AssignTime(seCourse2.Id, 6); AssignTime(seCourse2.Id, 17);
        // 计算机网络: 周三1-2节 + 周五3-4节
        AssignTime(csCourse1.Id, 11); AssignTime(csCourse1.Id, 22);
        // 编译原理: 周四1-2节
        AssignTime(csCourse2.Id, 16);
        // 数据挖掘: 周二3-4节
        AssignTime(dsCourse1.Id, 7);
        // 选修课 (安排在与专业课不同的时段，避免教师冲突)
        AssignTime(electives[0].Id, 3);  // 大学英语 周一5-6节 (张教授此时无课)
        AssignTime(electives[1].Id, 8);  // 高等数学 周二5-6节 (李教授此时无课)
        AssignTime(electives[2].Id, 13); // Python程序设计 周三5-6节 (王教授此时无课)
        AssignTime(electives[3].Id, 18); // 人工智能导论 周四5-6节 (赵教授此时无课)
        AssignTime(electives[4].Id, 4);  // 数据库原理 周一7-8节 (陈教授此时无课)
        AssignTime(electives[5].Id, 23); // 软件工程导论 周五5-6节 (张教授此时无课)
        db.SaveChanges();

        // ===== 先修课关系 =====
        // 数据挖掘需要先修 数据库原理
        db.Prerequisites.Add(new Prerequisite { CourseId = dsCourse1.Id, PrereqCourseId = electives[4].Id });
        // 编译原理需要先修 数据结构
        db.Prerequisites.Add(new Prerequisite { CourseId = csCourse2.Id, PrereqCourseId = seCourse1.Id });
        db.SaveChanges();

        Console.WriteLine($"种子数据初始化完成！");
        Console.WriteLine($"  专业: {majors.Count}个");
        Console.WriteLine($"  管理员: admin / admin123");
        Console.WriteLine($"  教师: {teachers.Count}人 (密码: 123456)");
        Console.WriteLine($"  学生: {students.Count}人 (密码: 123456)");
        Console.WriteLine($"  教室: {classrooms.Count}间");
        Console.WriteLine($"  课程: {db.Courses.Count()}门 (含选修)");
        Console.WriteLine($"  时间片: {slots.Count}个");
    }
}
