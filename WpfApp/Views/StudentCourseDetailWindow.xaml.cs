using BusinessObjects;
using Services;
using System.Windows;
using System.Windows.Input;

namespace WpfApp.View
{
    public partial class StudentCourseDetailWindow : Window
    {
        private readonly IStudentCourseService _service;
        private int _courseId;
        private string _courseName;
        public StudentCourseDetailWindow(int courseId)
        {
            InitializeComponent();
            _service = new StudentCourseService();
            _courseId = courseId;
            LoadCourse(courseId);
        }

        private void LoadCourse(int courseId)
        {
            var course = _service.GetCourseById(courseId);

            if (course == null)
            {
                MessageBox.Show("Course not found!");
                this.Close();
                return;
            }
            _courseName = course.CourseName;

            txtCourseName.Text = course.CourseName;
            txtCourseCode.Text = "Code: " + course.CourseCode;
            txtCategory.Text = course.Category;
            txtDuration.Text = course.DurationWeeks + " weeks";
            txtFee.Text = course.Fee.ToString("N0");
            txtStatus.Text = course.Status;
            txtDescription.Text = course.Description;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCheckAvailability_Click(object sender, RoutedEventArgs e)
        {
            StudentClassWindow window = new StudentClassWindow(_courseId, _courseName);
            window.ShowDialog();
        }
    }
}