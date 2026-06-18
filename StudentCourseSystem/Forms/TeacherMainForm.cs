using Microsoft.EntityFrameworkCore;
using StudentCourseSystem.Data;
using StudentCourseSystem.Models;
using StudentCourseSystem.Services;

namespace StudentCourseSystem.Forms;

public class TeacherMainForm : Form
{
    private readonly Teacher _teacher;
    private readonly EnrollmentService _enrollSvc = new();
    private readonly CourseService _courseSvc = new();
    private readonly ComboBox _cmbCourse = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly DataGridView _dgvStudents = new();
    private readonly Label _lblInfo = new();
    private int _currentSemesterId;

    public TeacherMainForm(Teacher teacher)
    {
        _teacher = teacher;
        Text = $"教师端 - {teacher.Name}";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 600);

        using var db = new AppDbContext();
        _currentSemesterId = db.Semesters.OrderByDescending(s => s.StartDate).First().Id;

        // 顶部
        var topPanel = new Panel { Height = 50, Dock = DockStyle.Top, BackColor = Color.LightSteelBlue, Padding = new Padding(10) };
        topPanel.Controls.Add(new Label
        {
            Text = $"  {teacher.Name} - {teacher.Major?.Name ?? ""}",
            Font = new Font("Microsoft YaHei", 12),
            AutoSize = true,
            Dock = DockStyle.Left
        });

        // 课程选择
        var selPanel = new Panel { Height = 45, Dock = DockStyle.Top, Padding = new Padding(10) };
        selPanel.Controls.Add(new Label { Text = "选择课程: ", AutoSize = true, Dock = DockStyle.Left, TextAlign = ContentAlignment.MiddleLeft });
        selPanel.Controls.Add(_cmbCourse);
        _cmbCourse.Dock = DockStyle.Left;
        _cmbCourse.SelectedIndexChanged += (_, _) => LoadStudents();

        // 学生列表
        SetupGrid(_dgvStudents, ("学号", "StudentNo"), ("姓名", "StudentName"), ("专业", "MajorName"), ("成绩", "GradeStr"));
        _dgvStudents.CellEndEdit += (_, e) =>
        {
            var enrollmentId = Convert.ToInt32(_dgvStudents.Rows[e.RowIndex].Cells["Id"].Value);
            if (int.TryParse(_dgvStudents.Rows[e.RowIndex].Cells["GradeStr"].Value?.ToString(), out var grade))
                _enrollSvc.SetGrade(enrollmentId, grade);
        };

        // 底部按钮
        var bottomPanel = new Panel { Height = 40, Dock = DockStyle.Bottom };
        var btnSave = new Button { Text = "保存所有成绩", Dock = DockStyle.Right, Width = 150, BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnSave.Click += (_, _) =>
        {
            foreach (DataGridViewRow row in _dgvStudents.Rows)
            {
                if (row.IsNewRow) continue;
                var enrollmentId = Convert.ToInt32(row.Cells["Id"].Value);
                if (int.TryParse(row.Cells["GradeStr"].Value?.ToString(), out var grade))
                    _enrollSvc.SetGrade(enrollmentId, grade);
            }
            MessageBox.Show("成绩已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        bottomPanel.Controls.Add(btnSave);

        Controls.Add(_dgvStudents);
        Controls.Add(selPanel);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        LoadCourses();
    }

    private void SetupGrid(DataGridView grid, params (string Header, string Prop)[] cols)
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoGenerateColumns = false;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "Id", Width = 0 });
        foreach (var (h, p) in cols)
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, DataPropertyName = p, Name = p });
    }

    private void LoadCourses()
    {
        var courses = _courseSvc.GetTeacherCourses(_teacher.Id, _currentSemesterId);
        _cmbCourse.Items.Clear();
        foreach (var c in courses)
            _cmbCourse.Items.Add(new { c.Id, c.Name, ClassroomName = c.Classroom?.Name ?? "", TimeLabels = string.Join(",", c.CourseTimeSlots?.Select(ct => ct.TimeSlot?.Label ?? "") ?? Array.Empty<string>()) });
        _cmbCourse.DisplayMember = "Name";
        if (_cmbCourse.Items.Count > 0) _cmbCourse.SelectedIndex = 0;
    }

    private void LoadStudents()
    {
        if (_cmbCourse.SelectedItem == null) return;
        var courseInfo = (dynamic)_cmbCourse.SelectedItem;
        var enrolls = _enrollSvc.GetCourseStudents((int)courseInfo.Id);
        var data = enrolls.Select(e => new
        {
            e.Id,
            StudentNo = e.Student?.StudentNo ?? "",
            StudentName = e.Student?.Name ?? "",
            MajorName = e.Student?.Major?.Name ?? "",
            GradeStr = e.Grade?.ToString() ?? ""
        }).ToList();
        _dgvStudents.DataSource = new BindingSource { DataSource = data };
    }
}
