using BusinessObjects;
using Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfApp
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthService _authService;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(Login);
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password { get; set; } = "";

        public ICommand LoginCommand { get; }

        private void Login()
        {
            var result = _authService.Login(Email, Password);

            if (result.IsSuccess)
            {
                Session.Role = result.Role!;
                Session.User = result.User;

                MessageBox.Show($"Login thành công: {result.Role}");

                // Mở màn hình theo role
                switch (result.Role)
                {
                    case "Admin":
                        new AdminWindowTestLogin().Show();
                        break;
                    case "Teacher":
                        new TeacherWindowTestLogin().Show();
                        break;
                    case "Student":
                        new StudentWindowTestLogin().Show();
                        break;
                }

                // đóng login window
                Application.Current.Windows[0].Close();
            }
            else
            {
                MessageBox.Show(result.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
