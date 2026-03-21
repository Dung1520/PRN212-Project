using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.View
{
    public partial class StudentCourseWindow : Window
    {
        private readonly IStudentCourseService _studentCourseService;

        public StudentCourseWindow()
        {
            InitializeComponent();
            _studentCourseService = new StudentCourseService();
            LoadCourses();
        }

        private void LoadCourses()
        {
            string keyword = txtSearch?.Text?.Trim() ?? string.Empty;
            string status = (cbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";

            var courses = _studentCourseService.GetCourses(keyword, status);
            dgCourses.ItemsSource = courses;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadCourses();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;

            // Reset ComboBox về "All"
            cbStatus.SelectedIndex = 0;

            LoadCourses();
        }

        private void cbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCourses != null)
            {
                LoadCourses();
            }
        }

        private void BtnAvailableClasses_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentCourseListDto item)
            {
                int courseId = item.Id;
                string courseName = item.CourseName;

                var window = new StudentClassWindow(courseId, courseName);
                window.ShowDialog();
            }
        }

        private void BtnCourseDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int courseId))
            {
                var window = new StudentCourseDetailWindow(courseId);
                window.ShowDialog();
            }
        }
    }
}