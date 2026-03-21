using System.Windows;
using Services;

namespace WpfApp.Views.Admin
{
    public partial class TeacherDetailWindow : Window
    {
        private readonly ITeacherService _teacherService;
        private readonly int _teacherId;

        public TeacherDetailWindow(int teacherId)
        {
            InitializeComponent();
            _teacherService = new TeacherService();
            _teacherId = teacherId;
            LoadDetail();
        }

        private void LoadDetail()
        {
            var teacher = _teacherService.GetTeacherById(_teacherId);

            if (teacher == null)
            {
                MessageBox.Show("Teacher not found.");
                Close();
                return;
            }

            txtTeacherCode.Text = teacher.TeacherCode;
            txtUsername.Text = teacher.Username;
            txtFullName.Text = teacher.FullName;
            txtEmail.Text = teacher.Email;
            txtPhoneNumber.Text = teacher.PhoneNumber ?? "";
            txtGender.Text = teacher.Gender ?? "";
            txtDateOfBirth.Text = teacher.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
            txtAddress.Text = teacher.Address ?? "";
            txtIsActive.Text = teacher.IsActive ? "Active" : "Inactive";
            txtCreatedAt.Text = teacher.CreatedAt.ToString("dd/MM/yyyy HH:mm");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}