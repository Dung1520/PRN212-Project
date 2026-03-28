using Services;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class AdminPeopleView : UserControl
    {
        private readonly ITeacherService _teacherService = new TeacherService();
        private readonly IStudentService _studentService = new StudentService();

        public AdminPeopleView()
        {
            InitializeComponent();
            LoadAll();
        }

        private void LoadAll()
        {
            LoadStudents();
            LoadTeachers();
            HideStudentDetail();
            HideTeacherDetail();
        }

        private void LoadStudents()
        {
            StudentGrid.ItemsSource = _studentService.GetStudentList(StudentKeywordTextBox?.Text?.Trim());
        }

        private void LoadTeachers()
        {
            TeacherGrid.ItemsSource = _teacherService.GetTeacherList(TeacherKeywordTextBox?.Text?.Trim());
        }

        private void SearchStudents_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
            HideStudentDetail();
        }

        private void SearchTeachers_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
            HideTeacherDetail();
        }

        private void ReloadStudents_Click(object sender, RoutedEventArgs e)
        {
            StudentKeywordTextBox.Text = string.Empty;
            LoadStudents();
            HideStudentDetail();
        }

        private void ReloadTeachers_Click(object sender, RoutedEventArgs e)
        {
            TeacherKeywordTextBox.Text = string.Empty;
            LoadTeachers();
            HideTeacherDetail();
        }

        private void StudentDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;

            if (!int.TryParse(button.Tag.ToString(), out var studentId))
                return;

            var detail = _studentService.GetStudentDetailById(studentId);
            if (detail == null)
            {
                MessageBox.Show("Không tìm thấy thông tin student.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            StudentInfoText.Text =
                $"Code: {detail.StudentCode}\n" +
                $"Username: {detail.Username}\n" +
                $"Full Name: {detail.FullName}\n" +
                $"Email: {detail.Email}\n" +
                $"Phone: {detail.PhoneNumber}\n" +
                $"Gender: {detail.Gender}\n" +
                $"Date of Birth: {(detail.DateOfBirth.HasValue ? detail.DateOfBirth.Value.ToString("dd/MM/yyyy") : string.Empty)}\n" +
                $"Address: {detail.Address}\n" +
                $"Active: {detail.IsActive}\n" +
                $"Created At: {detail.CreatedAt:dd/MM/yyyy HH:mm}";

            StudentSummaryText.Text =
                $"Pending: {detail.PendingCount} | Approved: {detail.ApprovedCount} | Rejected: {detail.RejectedCount}";

            StudentEnrollmentGrid.ItemsSource = detail.Enrollments;
            StudentDetailPanel.Visibility = Visibility.Visible;
        }

        private void TeacherDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag == null)
                return;

            if (!int.TryParse(button.Tag.ToString(), out var teacherId))
                return;

            var detail = _teacherService.GetTeacherDetailById(teacherId);
            if (detail == null)
            {
                MessageBox.Show("Không tìm thấy thông tin teacher.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TeacherInfoText.Text =
                $"Code: {detail.TeacherCode}\n" +
                $"Username: {detail.Username}\n" +
                $"Full Name: {detail.FullName}\n" +
                $"Email: {detail.Email}\n" +
                $"Phone: {detail.PhoneNumber}\n" +
                $"Gender: {detail.Gender}\n" +
                $"Date of Birth: {(detail.DateOfBirth.HasValue ? detail.DateOfBirth.Value.ToString("dd/MM/yyyy") : string.Empty)}\n" +
                $"Address: {detail.Address}\n" +
                $"Active: {detail.IsActive}\n" +
                $"Created At: {detail.CreatedAt:dd/MM/yyyy HH:mm}";

            TeacherSummaryText.Text =
                $"Teaching classes: {detail.TotalTeachingClasses} | Open: {detail.OpenClassCount} | Full: {detail.FullClassCount} | Closed: {detail.ClosedClassCount}";

            TeacherClassGrid.ItemsSource = detail.TeachingClasses;
            TeacherDetailPanel.Visibility = Visibility.Visible;
        }

        private void CloseStudentDetail_Click(object sender, RoutedEventArgs e)
        {
            HideStudentDetail();
        }

        private void CloseTeacherDetail_Click(object sender, RoutedEventArgs e)
        {
            HideTeacherDetail();
        }

        private void HideStudentDetail()
        {
            StudentDetailPanel.Visibility = Visibility.Collapsed;
            StudentInfoText.Text = string.Empty;
            StudentSummaryText.Text = string.Empty;
            StudentEnrollmentGrid.ItemsSource = null;
        }

        private void HideTeacherDetail()
        {
            TeacherDetailPanel.Visibility = Visibility.Collapsed;
            TeacherInfoText.Text = string.Empty;
            TeacherSummaryText.Text = string.Empty;
            TeacherClassGrid.ItemsSource = null;
        }
    }
}