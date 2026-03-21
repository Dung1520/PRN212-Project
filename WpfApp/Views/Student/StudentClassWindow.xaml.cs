using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.View
{
    public partial class StudentClassWindow : Window
    {
        private readonly IStudentCourseService _service;
        private int _courseId;

        public StudentClassWindow(int courseId, string courseName)
        {
            InitializeComponent();

            _service = new StudentCourseService();
            _courseId = courseId;

            txtHeader.Text = $"Classes for: {courseName}";

            LoadClasses();
        }

        private void LoadClasses()
        {
            int studentId = 1; // hoặc session
            var classes = _service.GetClassesByCourseId(_courseId, studentId);

            if (classes.Count == 0)
            {
                txtNotice.Text = "Bạn đã đăng ký lớp trong course này!";
                txtNotice.Visibility = Visibility.Visible;
            }
            else
            {
                txtNotice.Visibility = Visibility.Collapsed;
            }

            dgClasses.ItemsSource = _service.GetClassesByCourseId(_courseId, studentId);
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentClassDto item)
            {
                int classId = item.Id;
                int studentId = 1;

                try
                {
                    if (item.EnrollmentStatus == "Pending")
                    {
                        _service.CancelEnrollment(studentId, classId);
                        MessageBox.Show("Đã hủy đăng ký!");
                    }
                    else
                    {
                        _service.RegisterClass(studentId, classId);
                        MessageBox.Show("Đăng ký thành công!");
                    }

                    LoadClasses();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgClasses_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}