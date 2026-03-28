using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp.Views
{
    public partial class LoginView : UserControl
    {
        private readonly IAuthService _authService = new AuthService();
        private readonly IStudentService _studentService = new StudentService();
        private readonly Action<LoginUser> _onLoginSuccess;

        public LoginView(Action<LoginUser> onLoginSuccess)
        {
            InitializeComponent();
            _onLoginSuccess = onLoginSuccess;
            ShowLoginPanel();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var user = _authService.Login(UsernameTextBox.Text, PasswordTextBox.Password);
            if (user == null)
            {
                MessageText.Text = "Sai tài khoản hoặc mật khẩu.";
                return;
            }

            MessageText.Text = string.Empty;
            _onLoginSuccess(user);
        }

        private void ShowRegisterButton_Click(object sender, RoutedEventArgs e)
            => ShowRegisterPanel();

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
            => ShowLoginPanel();

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGender = (RegisterGenderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            var request = new StudentRegistrationRequest
            {
                FullName = RegisterFullNameTextBox.Text,
                Username = RegisterUsernameTextBox.Text,
                Email = RegisterEmailTextBox.Text,
                Password = RegisterPasswordBox.Password,
                ConfirmPassword = RegisterConfirmPasswordBox.Password,
                PhoneNumber = RegisterPhoneTextBox.Text,
                DateOfBirth = RegisterDateOfBirthPicker.SelectedDate,
                Gender = string.IsNullOrWhiteSpace(selectedGender) ? null : selectedGender,
                Address = RegisterAddressTextBox.Text
            };

            var result = _studentService.RegisterStudent(request);

            RegisterMessageText.Foreground = result.IsSuccess
                ? new SolidColorBrush(Color.FromRgb(22, 163, 74))
                : Brushes.Crimson;

            RegisterMessageText.Text = result.Message;

            if (!result.IsSuccess)
                return;

            UsernameTextBox.Text = request.Username;
            PasswordTextBox.Password = request.Password;

            ClearRegisterForm();
            ShowLoginPanel();
            MessageText.Text = "Đăng ký thành công. Bạn có thể đăng nhập ngay bằng tài khoản vừa tạo.";
        }

        private void ShowLoginPanel()
        {
            HeaderText.Text = "Đăng nhập hệ thống";
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
            RegisterMessageText.Text = string.Empty;
        }

        private void ShowRegisterPanel()
        {
            HeaderText.Text = "Đăng ký Student";
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
            MessageText.Text = string.Empty;
        }

        private void ClearRegisterForm()
        {
            RegisterFullNameTextBox.Clear();
            RegisterUsernameTextBox.Clear();
            RegisterEmailTextBox.Clear();
            RegisterPasswordBox.Clear();
            RegisterConfirmPasswordBox.Clear();
            RegisterPhoneTextBox.Clear();
            RegisterDateOfBirthPicker.SelectedDate = null;
            RegisterGenderComboBox.SelectedIndex = 0;
            RegisterAddressTextBox.Clear();
        }
    }
}