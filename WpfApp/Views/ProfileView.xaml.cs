using BusinessObjects;
using Services;
using System.Text;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView(LoginUser user)
        {
            InitializeComponent();
            var sb = new StringBuilder();
            sb.AppendLine($"Role: {user.Role}");
            sb.AppendLine($"Username: {user.Username}");
            sb.AppendLine($"Full name: {user.FullName}");
            sb.AppendLine($"Email: {user.Email}");

            if (user.Role == "Student")
            {
                var s = new StudentService().GetStudentById(user.UserId);
                if (s != null)
                {
                    sb.AppendLine($"StudentCode: {s.StudentCode}");
                    sb.AppendLine($"Phone: {s.PhoneNumber}");
                    sb.AppendLine($"DOB: {s.DateOfBirth:dd/MM/yyyy}");
                    sb.AppendLine($"Gender: {s.Gender}");
                    sb.AppendLine($"Address: {s.Address}");
                }
            }
            else if (user.Role == "Teacher")
            {
                var t = new TeacherService().GetTeacherById(user.UserId);
                if (t != null)
                {
                    sb.AppendLine($"TeacherCode: {t.TeacherCode}");
                    sb.AppendLine($"Phone: {t.PhoneNumber}");
                    sb.AppendLine($"DOB: {t.DateOfBirth:dd/MM/yyyy}");
                    sb.AppendLine($"Gender: {t.Gender}");
                    sb.AppendLine($"Address: {t.Address}");
                }
            }
            else
            {
                sb.AppendLine("Admin mặc định đang đọc từ appsettings.json theo đúng đề bài.");
            }

            ProfileText.Text = sb.ToString();
        }
    }
}
