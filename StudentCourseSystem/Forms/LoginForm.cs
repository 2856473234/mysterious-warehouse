using StudentCourseSystem.Services;

namespace StudentCourseSystem.Forms;

public class LoginForm : Form
{
    private readonly ComboBox cmbRole = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtAccount = new();
    private readonly TextBox txtPassword = new() { PasswordChar = '*' };
    private readonly Button btnLogin = new() { Text = "登录", BackColor = Color.DodgerBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    private readonly Label lblMsg = new() { ForeColor = Color.Red };

    public LoginForm()
    {
        Text = "学生选课系统 - 登录";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(400, 320);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var tbl = new TableLayoutPanel
        {
            RowCount = 6, ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 20, 30, 20),
        };
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

        // 标题
        tbl.Controls.Add(new Label
        {
            Text = "学生选课系统",
            Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        }, 0, 0);
        tbl.SetColumnSpan(tbl.GetControlFromPosition(0, 0)!, 2);

        // 角色选择
        cmbRole.Items.AddRange(new[] { "学生", "教师", "管理员" });
        cmbRole.SelectedIndex = 0;
        tbl.Controls.Add(new Label { Text = "角色:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 1);
        tbl.Controls.Add(cmbRole, 1, 1);

        // 账号
        tbl.Controls.Add(new Label { Text = "账号:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 2);
        tbl.Controls.Add(txtAccount, 1, 2);

        // 密码
        tbl.Controls.Add(new Label { Text = "密码:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, 3);
        tbl.Controls.Add(txtPassword, 1, 3);

        // 登录按钮
        tbl.Controls.Add(btnLogin, 0, 4);
        tbl.SetColumnSpan(btnLogin, 2);
        btnLogin.Dock = DockStyle.Fill;
        btnLogin.Height = 35;

        tbl.Controls.Add(lblMsg, 0, 5);
        tbl.SetColumnSpan(lblMsg, 2);
        lblMsg.TextAlign = ContentAlignment.MiddleCenter;

        Controls.Add(tbl);

        btnLogin.Click += DoLogin;
        txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) DoLogin(null!, EventArgs.Empty); };
        txtAccount.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
        cmbRole.SelectedIndexChanged += UpdateRoleHint;
        UpdateRoleHint(null!, EventArgs.Empty);
    }

    private void DoLogin(object? sender, EventArgs e)
    {
        var account = txtAccount.Text.Trim();
        var password = txtPassword.Text.Trim();
        var role = cmbRole.Text;
        if (account == "" || password == "") { lblMsg.Text = "请输入账号和密码"; return; }

        var auth = new AuthService();
        var result = role switch
        {
            "学生" => auth.LoginAsStudent(account, password),
            "教师" => auth.LoginAsTeacher(account, password),
            "管理员" => auth.LoginAsAdmin(account, password),
            _ => null
        };

        if (result == null)
        {
            lblMsg.Text = "账号或密码错误";
            return;
        }

        var (user, _) = result.Value;
        Hide();

        switch (role)
        {
            case "学生":
                new StudentMainForm((Models.Student)user).ShowDialog();
                break;
            case "教师":
                new TeacherMainForm((Models.Teacher)user).ShowDialog();
                break;
            case "管理员":
                new AdminMainForm().ShowDialog();
                break;
        }

        txtPassword.Clear();
        Show();
    }

    private void UpdateRoleHint(object? sender, EventArgs e)
    {
        txtAccount.Clear();
        txtPassword.Clear();
        txtAccount.Focus();
        txtAccount.PlaceholderText = cmbRole.Text switch
        {
            "学生" => "学号 (如 2024SE01)",
            "教师" => "教师姓名",
            "管理员" => "用户名",
            _ => ""
        };
    }
}
