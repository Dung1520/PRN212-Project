using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class StudentCoursesView : UserControl
    {
        private readonly LoginUser _user;
        private readonly IStudentCourseService _service = new StudentCourseService();
        private int? _currentCourseId;

        public StudentCoursesView(LoginUser user)
        {
            InitializeComponent();
            _user = user;
            LoadCourses();
            ShowListOnly();
        }

        private void LoadCourses()
        {
            var status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content == null
                ? null
                : ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();

            CourseGrid.ItemsSource = _service.GetCourses(KeywordTextBox.Text.Trim(), status);
        }

        private void ShowListOnly()
        {
            BackToListButton.Visibility = Visibility.Collapsed;
            DetailPanel.Visibility = Visibility.Collapsed;
            ClassPanel.Visibility = Visibility.Collapsed;
            DetailText.Text = string.Empty;
            ClassGrid.ItemsSource = null;
            _currentCourseId = null;
        }

        private void ShowDetailPanel(int courseId)
        {
            var detail = _service.GetCourseById(courseId);
            if (detail == null)
            {
                MessageBox.Show("Không tìm thấy khóa học.");
                return;
            }

            _currentCourseId = courseId;
            BackToListButton.Visibility = Visibility.Visible;
            DetailPanel.Visibility = Visibility.Visible;
            ClassPanel.Visibility = Visibility.Collapsed;
            DetailText.Text =
                "Code: " + detail.CourseCode + "\n" +
                "Tên: " + detail.CourseName + "\n" +
                "Chủ đề: " + detail.Category + "\n" +
                "Số tuần: " + detail.DurationWeeks + "\n" +
                "Học phí: " + detail.Fee.ToString("0,0") + "\n" +
                "Trạng thái: " + detail.Status + "\n\n" +
                "Mô tả:\n" + detail.Description;
        }

        private void ShowAvailableClassPanel(int courseId)
        {
            _currentCourseId = courseId;
            BackToListButton.Visibility = Visibility.Visible;
            DetailPanel.Visibility = Visibility.Collapsed;
            ClassPanel.Visibility = Visibility.Visible;
            ClassGrid.ItemsSource = _service.GetClassesByCourseId(courseId, _user.UserId);
        }

        private int ResolveCourseIdFromButton(object sender)
        {
            return int.Parse(((Button)sender).Tag == null ? "0" : ((Button)sender).Tag.ToString());
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            LoadCourses();
            ShowListOnly();
        }

        private void CourseDetail_Click(object sender, RoutedEventArgs e)
        {
            var courseId = ResolveCourseIdFromButton(sender);
            ShowDetailPanel(courseId);
        }

        private void AvailableClass_Click(object sender, RoutedEventArgs e)
        {
            var courseId = ResolveCourseIdFromButton(sender);
            ShowAvailableClassPanel(courseId);
        }

        private void BackToListButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListOnly();
        }

        private void Enroll_Click(object sender, RoutedEventArgs e)
        {
            var classId = int.Parse(((Button)sender).Tag == null ? "0" : ((Button)sender).Tag.ToString());
            try
            {
                _service.RegisterClass(_user.UserId, classId);
                MessageBox.Show("Đăng ký thành công. Trạng thái ban đầu là Pending.");
                if (_currentCourseId.HasValue)
                {
                    ShowAvailableClassPanel(_currentCourseId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}