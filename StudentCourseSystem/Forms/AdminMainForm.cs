using StudentCourseSystem.Data;
using StudentCourseSystem.Models;
using StudentCourseSystem.Services;

namespace StudentCourseSystem.Forms;

public class AdminMainForm : Form
{
    private readonly AdminService _svc = new();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };

    // 教师管理
    private readonly DataGridView _dgvTeachers = new();
    // 课程管理
    private readonly DataGridView _dgvCourses = new();
    // 教室管理
    private readonly DataGridView _dgvClassrooms = new();
    // 学期管理
    private readonly DataGridView _dgvSemesters = new();
    // 排课管理
    private readonly ComboBox _cmbScheduleCourse = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckedListBox _clbTimeSlots = new();
    // 学生管理
    private readonly DataGridView _dgvStudents = new();

    public AdminMainForm()
    {
        Text = "管理员端 - 选课系统管理";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1200, 700);

        InitTeacherTab();
        InitCourseTab();
        InitClassroomTab();
        InitSemesterTab();
        InitScheduleTab();
        InitStudentTab();
        Controls.Add(_tabs);
    }

    // ===== 教师管理 =====
    private void InitTeacherTab()
    {
        var tab = new TabPage("教师管理");
        SetupGrid(_dgvTeachers, ("姓名", "Name"), ("所属学院", "MajorName"), ("密码", "Password"));
        _dgvTeachers.ReadOnly = true;

        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        var btnAdd = new Button { Text = "添加教师", Dock = DockStyle.Left, Width = 120 };
        var btnDel = new Button { Text = "删除选中", Dock = DockStyle.Left, Width = 120 };
        btnAdd.Click += (_, _) =>
        {
            var form = new TeacherEditForm(_svc.GetAllMajors());
            if (form.ShowDialog() == DialogResult.OK)
            {
                _svc.AddTeacher(form.Teacher);
                LoadTeachers();
            }
        };
        btnDel.Click += (_, _) =>
        {
            if (_dgvTeachers.CurrentRow != null && MessageBox.Show("确定删除？", "确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { _svc.DeleteTeacher(Convert.ToInt32(_dgvTeachers.CurrentRow.Cells["Id"]!.Value)); LoadTeachers(); }
        };
        pnl.Controls.AddRange(new Control[] { btnAdd, btnDel });

        tab.Controls.Add(_dgvTeachers);
        tab.Controls.Add(pnl);
        _tabs.TabPages.Add(tab);

        LoadTeachers();
    }
    private void LoadTeachers()
    {
        var teachers = _svc.GetAllTeachers();
        _dgvTeachers.DataSource = new BindingSource { DataSource = teachers.Select(t => new { t.Id, t.Name, MajorName = t.Major?.Name ?? "", t.Password }).ToList() };
    }

    // ===== 课程管理 =====
    private void InitCourseTab()
    {
        var tab = new TabPage("课程管理");
        SetupGrid(_dgvCourses,
            ("课程名称", "Name"), ("代码", "Code"), ("类型", "Type"), ("学分", "Credits"),
            ("教师", "TeacherName"), ("教室", "ClassroomName"), ("名额", "MaxStudents"),
            ("专业", "MajorName"), ("时间", "TimeLabels"));
        _dgvCourses.ReadOnly = true;

        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        var btnAdd = new Button { Text = "添加课程", Dock = DockStyle.Left, Width = 120 };
        var btnDel = new Button { Text = "删除选中", Dock = DockStyle.Left, Width = 120 };
        btnAdd.Click += (_, _) =>
        {
            using var db = new AppDbContext();
            var semesters = db.Semesters.OrderByDescending(s => s.StartDate).ToList();
            var teachers = _svc.GetAllTeachers();
            var classrooms = _svc.GetAllClassrooms();
            var majors = _svc.GetAllMajors();
            var form = new CourseEditForm(semesters, teachers, classrooms, majors);
            if (form.ShowDialog() == DialogResult.OK)
            {
                _svc.AddCourse(form.Course);
                LoadCourses();
            }
        };
        btnDel.Click += (_, _) =>
        {
            if (_dgvCourses.CurrentRow != null && MessageBox.Show("确定删除？", "确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { _svc.DeleteCourse(Convert.ToInt32(_dgvCourses.CurrentRow.Cells["Id"]!.Value)); LoadCourses(); }
        };
        pnl.Controls.AddRange(new Control[] { btnAdd, btnDel });

        tab.Controls.Add(_dgvCourses);
        tab.Controls.Add(pnl);
        _tabs.TabPages.Add(tab);

        LoadCourses();
    }
    private void LoadCourses()
    {
        using var db = new AppDbContext();
        var semId = db.Semesters.OrderByDescending(s => s.StartDate).First().Id;
        var courses = _svc.GetAllCourses(semId);
        _dgvCourses.DataSource = new BindingSource { DataSource = courses.Select(c => new
        {
            c.Id, c.Name, c.Code, c.Type, c.Credits,
            TeacherName = c.Teacher?.Name ?? "",
            ClassroomName = c.Classroom?.Name ?? "",
            c.MaxStudents, MajorName = c.Major?.Name ?? "全校选修",
            TimeLabels = string.Join(", ", c.CourseTimeSlots?.Select(ct => ct.TimeSlot?.Label ?? "") ?? Array.Empty<string>())
        }).ToList() };
    }

    // ===== 教室管理 =====
    private void InitClassroomTab()
    {
        var tab = new TabPage("教室管理");
        SetupGrid(_dgvClassrooms, ("教室名称", "Name"), ("座位数", "Capacity"));
        _dgvClassrooms.ReadOnly = true;

        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        var btnAdd = new Button { Text = "添加教室", Dock = DockStyle.Left, Width = 120 };
        var btnDel = new Button { Text = "删除选中", Dock = DockStyle.Left, Width = 120 };
        btnAdd.Click += (_, _) =>
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("教室名称:", "添加教室", "教301");
            if (!string.IsNullOrWhiteSpace(name))
            {
                var capStr = Microsoft.VisualBasic.Interaction.InputBox("座位数:", "添加教室", "60");
                if (int.TryParse(capStr, out var cap))
                { _svc.AddClassroom(new Classroom { Name = name, Capacity = cap }); LoadClassrooms(); }
            }
        };
        btnDel.Click += (_, _) =>
        {
            if (_dgvClassrooms.CurrentRow != null && MessageBox.Show("确定删除？", "确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { _svc.DeleteClassroom(Convert.ToInt32(_dgvClassrooms.CurrentRow.Cells["Id"]!.Value)); LoadClassrooms(); }
        };
        pnl.Controls.AddRange(new Control[] { btnAdd, btnDel });

        tab.Controls.Add(_dgvClassrooms);
        tab.Controls.Add(pnl);
        _tabs.TabPages.Add(tab);

        LoadClassrooms();
    }
    private void LoadClassrooms()
    {
        _dgvClassrooms.DataSource = new BindingSource { DataSource = _svc.GetAllClassrooms().Select(c => new { c.Id, c.Name, c.Capacity }).ToList() };
    }

    // ===== 学期管理 =====
    private void InitSemesterTab()
    {
        var tab = new TabPage("学期管理");
        SetupGrid(_dgvSemesters, ("名称", "Name"), ("开始日期", "StartDateStr"), ("结束日期", "EndDateStr"),
            ("选课开始", "EnrollStartStr"), ("选课结束", "EnrollEndStr"), ("周数", "WeekCount"));
        _dgvSemesters.ReadOnly = true;

        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        var btnAdd = new Button { Text = "添加学期", Dock = DockStyle.Left, Width = 120 };
        btnAdd.Click += (_, _) =>
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("学期名称:", "添加学期", "2026-2027第一学期");
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    var sem = new Semester
                    {
                        Name = name,
                        StartDate = DateTime.Parse(Microsoft.VisualBasic.Interaction.InputBox("开始日期 (yyyy-MM-dd):", "添加学期", "2026-09-01")),
                        EndDate = DateTime.Parse(Microsoft.VisualBasic.Interaction.InputBox("结束日期 (yyyy-MM-dd):", "添加学期", "2027-01-15")),
                        WeekCount = 16,
                        EnrollStart = DateTime.Parse(Microsoft.VisualBasic.Interaction.InputBox("选课开始 (yyyy-MM-dd):", "添加学期", "2026-08-25")),
                        EnrollEnd = DateTime.Parse(Microsoft.VisualBasic.Interaction.InputBox("选课结束 (yyyy-MM-dd):", "添加学期", "2026-09-10"))
                    };
                    _svc.AddSemester(sem);
                    LoadSemesters();
                }
                catch { MessageBox.Show("日期格式错误"); }
            }
        };
        pnl.Controls.Add(btnAdd);

        tab.Controls.Add(_dgvSemesters);
        tab.Controls.Add(pnl);
        _tabs.TabPages.Add(tab);

        LoadSemesters();
    }
    private void LoadSemesters()
    {
        var sems = _svc.GetAllSemesters();
        _dgvSemesters.DataSource = new BindingSource { DataSource = sems.Select(s => new
        {
            s.Id, s.Name,
            StartDateStr = s.StartDate.ToString("yyyy-MM-dd"),
            EndDateStr = s.EndDate.ToString("yyyy-MM-dd"),
            EnrollStartStr = s.EnrollStart.ToString("yyyy-MM-dd"),
            EnrollEndStr = s.EnrollEnd.ToString("yyyy-MM-dd"),
            s.WeekCount
        }).ToList() };
    }

    // ===== 排课管理 =====
    private void InitScheduleTab()
    {
        var tab = new TabPage("排课管理");
        _cmbScheduleCourse.Width = 300;

        var topPanel = new Panel { Height = 35, Dock = DockStyle.Top };
        topPanel.Controls.Add(new Label { Text = "课程: ", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft });
        topPanel.Controls.Add(_cmbScheduleCourse);
        _cmbScheduleCourse.Dock = DockStyle.Left;
        _cmbScheduleCourse.SelectedIndexChanged += (_, _) => LoadScheduleCheckList();

        _clbTimeSlots.Dock = DockStyle.Fill;
        _clbTimeSlots.CheckOnClick = true;
        _clbTimeSlots.ItemCheck += (_, e) =>
        {
            if (_cmbScheduleCourse.SelectedItem == null) return;
            var courseId = (int)((dynamic)_cmbScheduleCourse.SelectedItem).Id;
            var ts = (TimeSlot)_clbTimeSlots.Items[e.Index]!;
            if (e.NewValue == CheckState.Checked)
                _svc.AddCourseTimeSlot(courseId, ts.Id);
            else
                _svc.RemoveCourseTimeSlot(courseId, ts.Id);
        };

        var midPanel = new Panel { Height = 30, Dock = DockStyle.Bottom };
        var btnRefresh = new Button { Text = "刷新课程列表", Dock = DockStyle.Left, Width = 120 };
        btnRefresh.Click += (_, _) => LoadScheduleCourses();
        midPanel.Controls.Add(btnRefresh);

        tab.Controls.Add(_clbTimeSlots);
        tab.Controls.Add(topPanel);
        tab.Controls.Add(midPanel);
        _tabs.TabPages.Add(tab);

        LoadScheduleCourses();
    }
    private void LoadScheduleCourses()
    {
        using var db = new AppDbContext();
        var semId = db.Semesters.OrderByDescending(s => s.StartDate).First().Id;
        var courses = _svc.GetAllCourses(semId);
        _cmbScheduleCourse.Items.Clear();
        foreach (var c in courses)
            _cmbScheduleCourse.Items.Add(new { c.Id, Display = $"{c.Name} ({c.Teacher?.Name} - {c.Classroom?.Name})" });
        _cmbScheduleCourse.DisplayMember = "Display";
        if (_cmbScheduleCourse.Items.Count > 0) _cmbScheduleCourse.SelectedIndex = 0;
    }
    private void LoadScheduleCheckList()
    {
        if (_cmbScheduleCourse.SelectedItem == null) return;
        var courseId = (int)((dynamic)_cmbScheduleCourse.SelectedItem).Id;
        var allSlots = _svc.GetAllTimeSlots();
        using var db = new AppDbContext();
        var selectedIds = db.CourseTimeSlots.Where(ct => ct.CourseId == courseId).Select(ct => ct.TimeSlotId).ToHashSet();

        _clbTimeSlots.Items.Clear();
        foreach (var slot in allSlots)
        {
            var idx = _clbTimeSlots.Items.Add(slot, selectedIds.Contains(slot.Id));
        }
    }

    // ===== 学生管理 =====
    private void InitStudentTab()
    {
        var tab = new TabPage("学生管理");
        SetupGrid(_dgvStudents, ("学号", "StudentNo"), ("姓名", "Name"), ("专业", "MajorName"), ("年级", "Grade"));
        _dgvStudents.ReadOnly = true;
        tab.Controls.Add(_dgvStudents);
        _tabs.TabPages.Add(tab);
        LoadStudents();
    }
    private void LoadStudents()
    {
        var students = _svc.GetAllStudents();
        _dgvStudents.DataSource = new BindingSource { DataSource = students.Select(s => new { s.StudentNo, s.Name, MajorName = s.Major?.Name ?? "", s.Grade }).ToList() };
    }

    private static void SetupGrid(DataGridView grid, params (string Header, string Prop)[] cols)
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoGenerateColumns = false;
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Name = "Id", Width = 0 });
        foreach (var (h, p) in cols)
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, DataPropertyName = p, Name = p });
    }
}

// ===== 辅助窗体：添加教师 =====
public class TeacherEditForm : Form
{
    public Teacher Teacher = new();
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtPwd = new() { Text = "123456" };
    private readonly ComboBox _cmbMajor = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public TeacherEditForm(List<Major> majors)
    {
        Text = "添加教师";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(350, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var tbl = new TableLayoutPanel { RowCount = 4, ColumnCount = 2, Dock = DockStyle.Fill, Padding = new Padding(20) };
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        tbl.Controls.Add(new Label { Text = "姓名:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 0);
        tbl.Controls.Add(_txtName, 1, 0);
        tbl.Controls.Add(new Label { Text = "学院:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        tbl.Controls.Add(_cmbMajor, 1, 1);
        tbl.Controls.Add(new Label { Text = "密码:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
        tbl.Controls.Add(_txtPwd, 1, 2);

        var btnOk = new Button { Text = "确定", Dock = DockStyle.Fill, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("请输入姓名"); return; }
            Teacher = new Teacher
            {
                Name = _txtName.Text.Trim(),
                Password = _txtPwd.Text.Trim(),
                MajorId = ((Major)_cmbMajor.SelectedItem!).Id
            };
            DialogResult = DialogResult.OK;
            Close();
        };
        tbl.Controls.Add(btnOk, 0, 3);
        tbl.SetColumnSpan(btnOk, 2);

        _cmbMajor.DataSource = majors;
        _cmbMajor.DisplayMember = "Name";
        if (_cmbMajor.Items.Count > 0) _cmbMajor.SelectedIndex = 0;

        Controls.Add(tbl);
    }
}

// ===== 辅助窗体：添加课程 =====
public class CourseEditForm : Form
{
    public Course Course = new();
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtCode = new();
    private readonly ComboBox _cmbType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _numCredits = new() { Maximum = 10, Value = 3 };
    private readonly NumericUpDown _numMax = new() { Maximum = 200, Value = 60 };
    private readonly ComboBox _cmbTeacher = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbClassroom = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbMajor = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbSemester = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public CourseEditForm(List<Semester> sems, List<Teacher> teachers, List<Classroom> rooms, List<Major> majors)
    {
        Text = "添加课程";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(450, 350);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _cmbType.Items.AddRange(new[] { "选修", "专业" });
        _cmbType.SelectedIndex = 0;

        var tbl = new TableLayoutPanel
        {
            RowCount = 8, ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };
        for (int i = 0; i < 8; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        int r = 0;
        AddRow(tbl, r++, "课程名称:", _txtName);
        AddRow(tbl, r++, "课程代码:", _txtCode);
        AddRow(tbl, r++, "类型:", _cmbType);
        AddRow(tbl, r++, "学分:", _numCredits);
        AddRow(tbl, r++, "教师:", _cmbTeacher);
        AddRow(tbl, r++, "教室:", _cmbClassroom);
        AddRow(tbl, r++, "名额:", _numMax);
        AddRow(tbl, r++, "专业(专业课):", _cmbMajor);

        var btnOk = new Button { Text = "确定", Dock = DockStyle.Fill, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnOk.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("请输入课程名称"); return; }
            if (string.IsNullOrWhiteSpace(_txtCode.Text)) { MessageBox.Show("请输入课程代码"); return; }
            Course = new Course
            {
                Name = _txtName.Text.Trim(),
                Code = _txtCode.Text.Trim(),
                Credits = (int)_numCredits.Value,
                Type = _cmbType.Text,
                MaxStudents = (int)_numMax.Value,
                TeacherId = ((Teacher)_cmbTeacher.SelectedItem!).Id,
                ClassroomId = ((Classroom)_cmbClassroom.SelectedItem!).Id,
                MajorId = _cmbType.Text == "专业" ? ((Major)_cmbMajor.SelectedItem!).Id : null,
                SemesterId = ((Semester)_cmbSemester.SelectedItem!).Id
            };
            DialogResult = DialogResult.OK;
            Close();
        };

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        bottomPanel.Controls.Add(btnOk);

        _cmbTeacher.DataSource = teachers;
        _cmbTeacher.DisplayMember = "Name";
        _cmbClassroom.DataSource = rooms;
        _cmbClassroom.DisplayMember = "Name";
        var allMajor = new List<Major> { new() { Id = 0, Name = "(全校选修)" } };
        allMajor.AddRange(majors);
        _cmbMajor.DataSource = allMajor;
        _cmbMajor.DisplayMember = "Name";
        _cmbSemester.DataSource = sems;
        _cmbSemester.DisplayMember = "Name";

        Controls.Add(tbl);
        Controls.Add(bottomPanel);
    }

    private void AddRow(TableLayoutPanel tbl, int row, string label, Control ctrl)
    {
        tbl.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row);
        tbl.Controls.Add(ctrl, 1, row);
    }
}
