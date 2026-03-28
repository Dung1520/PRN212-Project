using BusinessObjects;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class StudentCoursesView : UserControl
    {
        private readonly LoginUser _user;
        private readonly IStudentCourseService _service = new StudentCourseService();
        private int? _currentCourseId;
        private readonly IAiCourseAdvisorService _aiService;

        public StudentCoursesView(LoginUser user)
        {
            InitializeComponent();
            _user = user;

            _aiService = new AiCourseAdvisorService(
                new StudentCourseService(),
                new LmStudioRecommendationProvider("phogpt-4b-chat"));

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

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    // Xóa cache các màn bị ảnh hưởng sau khi đăng ký
                    mainWindow.InvalidatePages("home", "courses", "registrations", "schedule");
                    mainWindow.NavigateTo("registrations", true);
                }
                else if (_currentCourseId.HasValue)
                {
                    ShowAvailableClassPanel(_currentCourseId.Value);
                }

                MessageBox.Show("Đăng ký thành công. Số liệu Tổng quan sẽ tự cập nhật khi bạn quay lại màn đó.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void AiRecommend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var prompt = AiPromptTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    MessageBox.Show("Vui lòng nhập nhu cầu học tập.");
                    return;
                }

                var result = await _aiService.RecommendForStudentAsync(_user.UserId, prompt);

                AiPanel.Visibility = Visibility.Visible;

                if (!result.Items.Any())
                {
                    AiResultTextBlock.Text = result.Summary;
                    return;
                }

                var lines = new List<string> { result.Summary, "" };

                foreach (var item in result.Items)
                {
                    var matched = FindCandidateById(item.CandidateId, _user.UserId);
                    if (matched != null)
                    {
                        lines.Add($"- {matched.ClassCode} | {matched.CourseName}");
                        lines.Add($"  Lý do: {item.Reason}");
                        lines.Add($"  Lịch: {matched.DayOfWeek}, {matched.Slot}");
                        lines.Add($"  Học phí: {matched.Fee:0,0}");
                        lines.Add("");
                    }
                }

                AiResultTextBlock.Text = string.Join(Environment.NewLine, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("AI suggestion failed: " + ex.Message);
            }
        }

        private AiRecommendationCandidateDto? FindCandidateById(int classId, int studentId)
        {
            var courses = _service.GetCourses(null, "Open");

            foreach (var course in courses)
            {
                if (_service.IsStudentAlreadyEnrolledInCourse(studentId, course.Id))
                    continue;

                var classes = _service.GetClassesByCourseId(course.Id, studentId);
                var cls = classes.FirstOrDefault(x => x.Id == classId);
                if (cls != null)
                {
                    return new AiRecommendationCandidateDto
                    {
                        CandidateId = cls.Id,
                        CourseId = course.Id,
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        Category = course.Category,
                        DurationWeeks = course.DurationWeeks,
                        Fee = course.Fee,
                        ClassId = cls.Id,
                        ClassCode = cls.ClassCode,
                        StartDate = cls.StartDate,
                        EndDate = cls.EndDate,
                        DayOfWeek = cls.DayOfWeek,
                        Slot = cls.Slot,
                        Capacity = cls.Capacity,
                        CurrentEnrollment = cls.CurrentEnrollment
                    };
                }
            }

            return null;
        }
    }
}