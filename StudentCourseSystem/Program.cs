using StudentCourseSystem.Forms;

namespace StudentCourseSystem;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // 首次运行自动创建数据库和种子数据
        DataSeeder.Seed();

        Application.Run(new LoginForm());
    }
}
