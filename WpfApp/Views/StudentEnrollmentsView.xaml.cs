using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class StudentEnrollmentsView : UserControl
    {
        private readonly LoginUser _user;
        private readonly IStudentCourseService _service = new StudentCourseService();

        public StudentEnrollmentsView(LoginUser user)
        {
            InitializeComponent();
            _user = user;
            LoadData();
        }

        private void LoadData() => EnrollmentGrid.ItemsSource = _service.GetStudentEnrollments(_user.UserId);

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var classId = int.Parse(((Button)sender).Tag.ToString()!);
            try
            {
                _service.CancelEnrollment(_user.UserId, classId);
                MessageBox.Show("Hủy đăng ký thành công.");
                LoadData();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}