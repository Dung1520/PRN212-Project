using System.Windows;
using Services;

namespace WpfApp.Views.Admin
{
    public partial class StudentDetailWindow : Window
    {
        private readonly IStudentService _studentService;
        private readonly int _studentId;

        public StudentDetailWindow(int studentId)
        {
            InitializeComponent();
            _studentService = new StudentService();
            _studentId = studentId;
            LoadDetail();
        }

        private void LoadDetail()
        {
            var student = _studentService.GetStudentById(_studentId);

            if (student == null)
            {
                MessageBox.Show("Student not found.");
                Close();
                return;
            }

            txtStudentCode.Text = student.StudentCode;
            txtUsername.Text = student.Username;
            txtFullName.Text = student.FullName;
            txtEmail.Text = student.Email;
            txtPhoneNumber.Text = student.PhoneNumber ?? "";
            txtGender.Text = student.Gender ?? "";
            txtDateOfBirth.Text = student.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
            txtAddress.Text = student.Address ?? "";
            txtIsActive.Text = student.IsActive ? "Active" : "Inactive";
            txtCreatedAt.Text = student.CreatedAt.ToString("dd/MM/yyyy HH:mm");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}