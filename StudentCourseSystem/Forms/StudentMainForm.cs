using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Services;

namespace StudentCourseSystem.Forms;

public class StudentMainForm : Form
{
    private readonly StudentCourseSystem.Models.Student _student;
    private readonly EnrollmentService _enrollSvc = new();
    private readonly CourseService _courseSvc = new();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _dgvAvailable = new();
    private readonly DataGridView _dgvEnrolled = new();
    private readonly DataGridView _dgvGrades = new();
    private readonly DataGridView _dgvSchedule = new();
    private int _currentSemesterId;
    private BindingSource _availableSource = new();

    public StudentMainForm(StudentCourseSystem.Models.Student student)
    {
        _student = student;
        Text = $"学生选课系统 - {student.Name} ({student.StudentNo})";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 650);

        // 获取当前学期（选课窗口内的学期，取最新的）
        using var db = new AppDbContext();
        var now = DateTime.Now;
        var semester = db.Semesters
            .Where(s => s.EnrollStart <= now && s.EnrollEnd >= now)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefault();
        semester ??= db.Semesters.OrderByDescending(s => s.StartDate).First();
        _currentSemesterId = semester.Id;

        var topPanel = new Panel { Height = 40, Dock = DockStyle.Top, BackColor = Color.LightSteelBlue };
        var lblWelcome = new Label
        {
            Text = $"  {student.Name} - {student.Major?.Name ?? ""}   |   当前学期: {semester.Name}",
            Dock = DockStyle.Left,
            AutoSize = false,
            Width = 500,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei", 11)
        };
        var btnRefresh = new Button { Text = "刷新", Dock = DockStyle.Right, Width = 80 };
        btnRefresh.Click += (_, _) => RefreshAll();
        topPanel.Controls.Add(lblWelcome);
        topPanel.Controls.Add(btnRefresh);

        InitTabs();
        Controls.Add(_tabs);
        Controls.Add(topPanel);

        RefreshAll();
    }

    private void InitTabs()
    {
        // Tab1 - 可选课程
        var tabAvailable = new TabPage("可选课程");
        SetupGrid(_dgvAvailable, false,
            ("课程名称", "Name"), ("代码", "Code"), ("类型", "Type"),
            ("学分", "Credits"), ("教师", "TeacherName"), ("教室", "ClassroomName"),
            ("名额", "MaxStudents"), ("已选人数", "EnrolledCount"), ("时间", "TimeLabels"));
        _dgvAvailable.ReadOnly = true;
        var btnEnroll = new Button { Text = "选课", BackColor = Color.DodgerBlue, ForeColor = Color.White, Dock = DockStyle.Bottom, Height = 40, Font = new Font("Microsoft YaHei", 11) };
        btnEnroll.Click += DoEnroll;
        tabAvailable.Controls.Add(_dgvAvailable);
        tabAvailable.Controls.Add(btnEnroll);

        // Tab2 - 我的课表
        var tabSchedule = new TabPage("我的课表");
        SetupGrid(_dgvSchedule, false, ("节次/星期", "DayOfWeek"), ("周一", "Mon"), ("周二", "Tue"), ("周三", "Wed"), ("周四", "Thu"), ("周五", "Fri"));
        _dgvSchedule.ReadOnly = true;
        _dgvSchedule.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabSchedule.Controls.Add(_dgvSchedule);

        // Tab3 - 已选课程
        var tabEnrolled = new TabPage("已选课程");
        SetupGrid(_dgvEnrolled, false,
            ("课程名称", "CourseName"), ("类型", "Type"), ("教师", "TeacherName"),
            ("教室", "ClassroomName"), ("状态", "Status"), ("选课时间", "EnrolledAtStr"));
        _dgvEnrolled.ReadOnly = true;
        var btnDrop = new Button { Text = "退课", BackColor = Color.OrangeRed, ForeColor = Color.White, Dock = DockStyle.Bottom, Height = 40, Font = new Font("Microsoft YaHei", 11) };
        btnDrop.Click += DoDrop;
        tabEnrolled.Controls.Add(_dgvEnrolled);
        tabEnrolled.Controls.Add(btnDrop);

        // Tab4 - 成绩查询
        var tabGrades = new TabPage("成绩查询");
        SetupGrid(_dgvGrades, false,
            ("课程名称", "CourseName"), ("学分", "Credits"), ("教师", "TeacherName"),
            ("学期", "SemesterName"), ("成绩", "GradeStr"), ("状态", "Status"));
        _dgvGrades.ReadOnly = true;
        tabGrades.Controls.Add(_dgvGrades);

        _tabs.TabPages.Add(tabAvailable);
        _tabs.TabPages.Add(tabSchedule);
        _tabs.TabPages.Add(tabEnrolled);
        _tabs.TabPages.Add(tabGrades);
    }

    private void SetupGrid(DataGridView grid, bool edit, params (string Header, string Prop)[] cols)
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = !edit;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        grid.AutoGenerateColumns = false;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Width = 0 });
        foreach (var (h, p) in cols)
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, DataPropertyName = p, Name = p });
    }

    private void RefreshAll()
    {
        RefreshAvailable();
        RefreshEnrolled();
        RefreshGrades();
        RefreshSchedule();
    }

    private void RefreshAvailable()
    {
        var courses = _courseSvc.GetAvailableCourses(_student.Id, _currentSemesterId);
        var data = courses.Select(c => new
        {
            c.Id, c.Name, c.Code, c.Type, c.Credits,
            TeacherName = c.Teacher?.Name ?? "",
            ClassroomName = c.Classroom?.Name ?? "",
            c.MaxStudents,
            EnrolledCount = c.Enrollments?.Count(e => e.Status == "已选") ?? 0,
            TimeLabels = string.Join(", ", c.CourseTimeSlots?.Select(ct => ct.TimeSlot?.Label ?? "") ?? Array.Empty<string>())
        }).ToList();
        _availableSource = new BindingSource { DataSource = data };
        _dgvAvailable.DataSource = _availableSource;
    }

    private void RefreshEnrolled()
    {
        var enrolls = _enrollSvc.GetStudentEnrollments(_student.Id, _currentSemesterId);
        var data = enrolls.Select(e => new
        {
            e.Id, e.CourseId,
            CourseName = e.Course?.Name ?? "",
            Type = e.Course?.Type ?? "",
            TeacherName = e.Course?.Teacher?.Name ?? "",
            ClassroomName = e.Course?.Classroom?.Name ?? "",
            e.Status,
            EnrolledAtStr = e.EnrolledAt.ToString("MM-dd HH:mm"),
            TimeLabels = string.Join(", ", e.Course?.CourseTimeSlots?.Select(ct => ct.TimeSlot?.Label ?? "") ?? Array.Empty<string>())
        }).ToList();
        _dgvEnrolled.DataSource = new BindingSource { DataSource = data };
    }

    private void RefreshGrades()
    {
        using var db = new AppDbContext();
        var enrolls = db.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Semester)
            .Where(e => e.StudentId == _student.Id)
            .OrderByDescending(e => e.Semester.StartDate)
            .ToList();
        var data = enrolls.Select(e => new
        {
            CourseName = e.Course?.Name ?? "",
            e.Course?.Credits,
            TeacherName = e.Course?.Teacher?.Name ?? "",
            SemesterName = e.Semester?.Name ?? "",
            GradeStr = e.Grade.HasValue ? e.Grade.Value.ToString() : (e.Status == "已选" ? "未录入" : "-"),
            e.Status
        }).ToList();
        _dgvGrades.DataSource = new BindingSource { DataSource = data };
    }

    private void RefreshSchedule()
    {
        var courses = _courseSvc.GetStudentCourses(_student.Id, _currentSemesterId);
        // 构建课表矩阵 [节次, 星期] -> 课程名
        var periods = new[] { "1-2节", "3-4节", "5-6节", "7-8节", "9-10节" };
        var days = new[] { "周一", "周二", "周三", "周四", "周五" };
        var matrix = new string[5, 5]; // [period, day]

        foreach (var course in courses)
        {
            foreach (var ct in course.CourseTimeSlots ?? Enumerable.Empty<StudentCourseSystem.Models.CourseTimeSlot>())
            {
                if (ct.TimeSlot == null) continue;
                var day = ct.TimeSlot.DayOfWeek - 1;
                var period = (ct.TimeSlot.StartPeriod - 1) / 2; // 1-2节->0, 3-4节->1, ...
                if (day >= 0 && day < 5 && period >= 0 && period < 5)
                {
                    matrix[period, day] = matrix[period, day] == ""
                        ? course.Name
                        : matrix[period, day] + "\n" + course.Name;
                }
            }
        }

        var rows = new List<object>();
        for (int p = 0; p < 5; p++)
        {
            var row = new Dictionary<string, object> { ["DayOfWeek"] = periods[p] };
            for (int d = 0; d < 5; d++)
                row[days[d]] = matrix[p, d];
            rows.Add(row);
        }

        // 手动填充网格
        _dgvSchedule.Columns.Clear();
        _dgvSchedule.Columns.Add("DayOfWeek", "节次");
        foreach (var d in days) _dgvSchedule.Columns.Add(d, d);
        _dgvSchedule.Rows.Clear();
        foreach (var row in rows)
        {
            var dict = (Dictionary<string, object>)row;
            var idx = _dgvSchedule.Rows.Add(dict["DayOfWeek"], dict["周一"], dict["周二"], dict["周三"], dict["周四"], dict["周五"]);
            _dgvSchedule.Rows[idx].Height = 80;
        }
    }

    private void DoEnroll(object? sender, EventArgs e)
    {
        if (_dgvAvailable.CurrentRow == null) return;
        var id = Convert.ToInt32(_dgvAvailable.CurrentRow.Cells["Id"].Value);
        var result = _enrollSvc.Enroll(_student.Id, id);
        if (result == "ok")
            MessageBox.Show("选课成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
            MessageBox.Show(result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        RefreshAll();
    }

    private void DoDrop(object? sender, EventArgs e)
    {
        if (_dgvEnrolled.CurrentRow == null) return;
        var enrollmentId = Convert.ToInt32(_dgvEnrolled.CurrentRow.Cells["Id"].Value);
        var courseName = _dgvEnrolled.CurrentRow.Cells["CourseName"].Value?.ToString() ?? "";
        if (MessageBox.Show($"确定退课「{courseName}」吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        var result = _enrollSvc.Drop(_student.Id, enrollmentId);
        MessageBox.Show(result == "ok" ? "退课成功" : result, "提示");
        RefreshAll();
    }
}
