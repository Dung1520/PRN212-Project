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
            StudentGrid.ItemsSource = _studentService.GetStudentList();
            TeacherGrid.ItemsSource = _teacherService.GetTeacherList();
        }

        private void SearchStudents_Click(object sender, RoutedEventArgs e) => StudentGrid.ItemsSource = _studentService.GetStudentList(StudentKeywordTextBox.Text.Trim());
        private void SearchTeachers_Click(object sender, RoutedEventArgs e) => TeacherGrid.ItemsSource = _teacherService.GetTeacherList(TeacherKeywordTextBox.Text.Trim());
    }
}
