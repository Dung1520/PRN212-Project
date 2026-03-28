using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class ProfileView : UserControl
    {
        private readonly LoginUser _user;
        private readonly Action? _refreshHeader;
        private readonly StudentService _studentService = new StudentService();
        private readonly TeacherService _teacherService = new TeacherService();

        public ProfileView(LoginUser user, Action? refreshHeader = null)
        {
            InitializeComponent();
            _user = user;
            _refreshHeader = refreshHeader;

            LoadProfile();
        }

        private void LoadProfile()
        {
            MessageTextBlock.Text = string.Empty;

            RoleText.Text = _user.Role;
            UsernameText.Text = _user.Username;
            CurrentEmailText.Text = _user.Email;

            if (_user.Role == "Student")
            {
                var student = _studentService.GetStudentById(_user.UserId);
                if (student == null)
                {
                    MessageTextBlock.Text = "Không tải được hồ sơ sinh viên.";
                    SetEditEnabled(false);
                    return;
                }

                CodeText.Text = student.StudentCode;
                NoteText.Text = "Bạn chỉ được sửa hồ sơ của chính mình. Không được sửa Username/StudentCode.";
                BindStudent(student);
                SetEditEnabled(true);
            }
            else if (_user.Role == "Teacher")
            {
                var teacher = _teacherService.GetTeacherById(_user.UserId);
                if (teacher == null)
                {
                    MessageTextBlock.Text = "Không tải được hồ sơ giáo viên.";
                    SetEditEnabled(false);
                    return;
                }

                CodeText.Text = teacher.TeacherCode;
                NoteText.Text = "Bạn chỉ được sửa hồ sơ của chính mình. Không được sửa Username/TeacherCode.";
                BindTeacher(teacher);
                SetEditEnabled(true);
            }
            else
            {
                CodeText.Text = "N/A";
                NoteText.Text = "Admin mặc định đang đọc từ appsettings.json theo đúng đề bài nên không cho chỉnh sửa tại đây.";
                FullNameTextBox.Text = _user.FullName;
                EmailTextBox.Text = _user.Email;
                PhoneTextBox.Text = string.Empty;
                DobDatePicker.SelectedDate = null;
                GenderComboBox.SelectedIndex = 0;
                AddressTextBox.Text = string.Empty;
                SetEditEnabled(false);
            }
        }

        private void BindStudent(Student student)
        {
            FullNameTextBox.Text = student.FullName ?? string.Empty;
            EmailTextBox.Text = student.Email ?? string.Empty;
            PhoneTextBox.Text = student.PhoneNumber ?? string.Empty;
            DobDatePicker.SelectedDate = student.DateOfBirth;
            SetGender(student.Gender);
            AddressTextBox.Text = student.Address ?? string.Empty;
        }

        private void BindTeacher(Teacher teacher)
        {
            FullNameTextBox.Text = teacher.FullName ?? string.Empty;
            EmailTextBox.Text = teacher.Email ?? string.Empty;
            PhoneTextBox.Text = teacher.PhoneNumber ?? string.Empty;
            DobDatePicker.SelectedDate = teacher.DateOfBirth;
            SetGender(teacher.Gender);
            AddressTextBox.Text = teacher.Address ?? string.Empty;
        }

        private void SetGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
            {
                GenderComboBox.SelectedIndex = 0;
                return;
            }

            foreach (var item in GenderComboBox.Items)
            {
                if (item is ComboBoxItem cbo &&
                    string.Equals(cbo.Content?.ToString(), gender, StringComparison.OrdinalIgnoreCase))
                {
                    GenderComboBox.SelectedItem = cbo;
                    return;
                }
            }

            GenderComboBox.SelectedIndex = 0;
        }

        private string? GetSelectedGender()
        {
            if (GenderComboBox.SelectedItem is ComboBoxItem item)
            {
                var value = item.Content?.ToString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return null;
        }

        private void SetEditEnabled(bool enabled)
        {
            FullNameTextBox.IsEnabled = enabled;
            EmailTextBox.IsEnabled = enabled;
            PhoneTextBox.IsEnabled = enabled;
            DobDatePicker.IsEnabled = enabled;
            GenderComboBox.IsEnabled = enabled;
            AddressTextBox.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProfile();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            MessageTextBlock.Text = string.Empty;

            if (_user.Role == "Student")
            {
                var current = _studentService.GetStudentById(_user.UserId);
                if (current == null)
                {
                    MessageTextBlock.Text = "Không tìm thấy hồ sơ sinh viên.";
                    return;
                }

                current.FullName = FullNameTextBox.Text;
                current.Email = EmailTextBox.Text;
                current.PhoneNumber = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
                current.DateOfBirth = DobDatePicker.SelectedDate;
                current.Gender = GetSelectedGender();
                current.Address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();

                var result = _studentService.UpdateOwnProfile(current);
                if (!result.IsSuccess)
                {
                    MessageTextBlock.Text = result.Message;
                    return;
                }

                _user.FullName = current.FullName;
                _user.Email = current.Email;
                CurrentEmailText.Text = _user.Email;
                _refreshHeader?.Invoke();

                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                MessageTextBlock.Text = result.Message;
                return;
            }

            if (_user.Role == "Teacher")
            {
                var current = _teacherService.GetTeacherById(_user.UserId);
                if (current == null)
                {
                    MessageTextBlock.Text = "Không tìm thấy hồ sơ giáo viên.";
                    return;
                }

                current.FullName = FullNameTextBox.Text;
                current.Email = EmailTextBox.Text;
                current.PhoneNumber = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
                current.DateOfBirth = DobDatePicker.SelectedDate;
                current.Gender = GetSelectedGender();
                current.Address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();

                var result = _teacherService.UpdateOwnProfile(current);
                if (!result.IsSuccess)
                {
                    MessageTextBlock.Text = result.Message;
                    return;
                }

                _user.FullName = current.FullName;
                _user.Email = current.Email;
                CurrentEmailText.Text = _user.Email;
                _refreshHeader?.Invoke();

                MessageTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                MessageTextBlock.Text = result.Message;
                return;
            }

            MessageTextBlock.Text = "Tài khoản admin mặc định không hỗ trợ chỉnh sửa tại màn hình này.";
        }
    }
}