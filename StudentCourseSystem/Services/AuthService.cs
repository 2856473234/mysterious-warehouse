using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;

namespace StudentCourseSystem.Services;

public class AuthService
{
    public (object User, string Role)? Login(string account, string password)
    {
        // 通用登录：自动检测角色
        var s = LoginAsStudent(account, password);
        if (s != null) return s;
        var t = LoginAsTeacher(account, password);
        if (t != null) return t;
        return LoginAsAdmin(account, password);
    }

    public (object User, string Role)? LoginAsStudent(string account, string password)
    {
        using var db = new AppDbContext();
        var s = db.Students.Include(x => x.Major).FirstOrDefault(x => x.StudentNo == account && x.Password == password);
        return s != null ? (s, "学生") : null;
    }

    public (object User, string Role)? LoginAsTeacher(string account, string password)
    {
        using var db = new AppDbContext();
        var t = db.Teachers.Include(x => x.Major).FirstOrDefault(x => x.Name == account && x.Password == password);
        return t != null ? (t, "教师") : null;
    }

    public (object User, string Role)? LoginAsAdmin(string account, string password)
    {
        using var db = new AppDbContext();
        var a = db.Admins.FirstOrDefault(x => x.Username == account && x.Password == password);
        return a != null ? (a, "管理员") : null;
    }
}
