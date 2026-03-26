using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class LoginView : UserControl
    {
        private readonly IAuthService _authService = new AuthService();
        private readonly Action<LoginUser> _onLoginSuccess;

        public LoginView(Action<LoginUser> onLoginSuccess)
        {
            InitializeComponent();
            _onLoginSuccess = onLoginSuccess;
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
    }
}
