using BusinessObjects;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace WpfApp.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView(LoginUser user)
        {
            InitializeComponent();
            TitleText.Text = $"Xin chào, {user.FullName}";
            SubtitleText.Text = user.Role switch
            {
                "Admin" => "Tổng quan nhanh toàn hệ thống.",
                "Teacher" => "Thông tin lớp giảng dạy và lịch dạy của bạn.",
                _ => "Thông tin khóa học, đăng ký và lịch học của bạn."
            };

            using var db = DbContextFactory.CreateDbContext();
            if (user.Role == "Admin")
            {
                AddStat("Khóa học", db.Courses.Count().ToString());
                AddStat("Lớp học", db.Classes.Count().ToString());
                AddStat("Đăng ký chờ duyệt", db.Enrollments.Count(x => x.Status == "Pending").ToString());
                AddStat("Tài khoản", (db.Students.Count() + db.Teachers.Count()).ToString());
                InfoText.Text = "Admin có thể quản lý Course, Class, duyệt Enrollment, xem danh sách người dùng và kiểm tra thời khóa biểu ngay trong menu bên trái.";
            }
            else if (user.Role == "Teacher")
            {
                var classCount = db.Classes.Count(x => x.TeacherId == user.UserId);
                var activeCount = db.Classes.Count(x => x.TeacherId == user.UserId && x.Status == "Open");
                var studentCount = (from e in db.Enrollments
                                    join c in db.Classes on e.ClassId equals c.Id
                                    where c.TeacherId == user.UserId && e.Status == "Approved"
                                    select e.StudentId).Distinct().Count();
                AddStat("Lớp phụ trách", classCount.ToString());
                AddStat("Đang mở", activeCount.ToString());
                AddStat("Học viên", studentCount.ToString());
                AddStat("Tuần hiện tại", System.DateTime.Today.ToString("dd/MM/yyyy"));
                InfoText.Text = "Teacher dùng menu Lịch giảng dạy để xem toàn bộ lịch theo tuần. Hồ sơ đang để chế độ xem nhanh từ DB.";
            }
            else
            {
                var pending = db.Enrollments.Count(x => x.StudentId == user.UserId && x.Status == "Pending");
                var approved = db.Enrollments.Count(x => x.StudentId == user.UserId && x.Status == "Approved");
                var total = db.Enrollments.Count(x => x.StudentId == user.UserId);
                AddStat("Đơn đã tạo", total.ToString());
                AddStat("Đang chờ", pending.ToString());
                AddStat("Được duyệt", approved.ToString());
                AddStat("Ngày hôm nay", System.DateTime.Today.ToString("dd/MM/yyyy"));
                InfoText.Text = "Student có thể tìm khóa học, xem lớp khả dụng, gửi đăng ký và hủy khi trạng thái còn Pending.";
            }
        }

        private void AddStat(string title, string value)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(16)
            };
            border.Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = title, Foreground = Brushes.Gray },
                    new TextBlock { Text = value, FontSize = 24, FontWeight = FontWeights.Bold }
                }
            };
            StatGrid.Children.Add(border);
        }
    }
}
