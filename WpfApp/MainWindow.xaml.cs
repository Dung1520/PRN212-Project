using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp.Views;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private LoginUser? _currentUser;
        private readonly Dictionary<string, UserControl> _cache = new();

        public MainWindow()
        {
            InitializeComponent();
            ShowLogin();
        }

        private void ShowLogin()
        {
            _currentUser = null;
            UserInfoText.Text = "Chưa đăng nhập";
            NavListBox.ItemsSource = null;
            NavListBox.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Collapsed;
            MainContent.Content = new LoginView(OnLoginSuccess);
        }

        private void OnLoginSuccess(LoginUser user)
        {
            _currentUser = user;
            _cache.Clear();
            RefreshUserInfo();
            NavListBox.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Visible;

            var menu = BuildMenu(user.Role);
            NavListBox.ItemsSource = menu;
            NavListBox.DisplayMemberPath = "Title";
            NavListBox.SelectedIndex = 0;
        }

        private void RefreshUserInfo()
        {
            if (_currentUser == null)
            {
                UserInfoText.Text = "Chưa đăng nhập";
                return;
            }

            UserInfoText.Text = $"{_currentUser.FullName}\nRole: {_currentUser.Role}\n{_currentUser.Email}";
        }

        private List<MenuItemVm> BuildMenu(string role)
        {
            return role switch
            {
                "Admin" => new List<MenuItemVm>
                {
                    new("home","Tổng quan"),
                    new("courses","Quản lý khóa học"),
                    new("classes","Quản lý lớp học"),
                    new("registrations","Duyệt đăng ký"),
                    new("people","Sinh viên & giáo viên"),
                    new("schedule","Thời khóa biểu"),
                    new("profile","Hồ sơ")
                },
                "Teacher" => new List<MenuItemVm>
                {
                    new("home","Tổng quan"),
                    new("schedule","Lịch giảng dạy"),
                    new("profile","Hồ sơ")
                },
                _ => new List<MenuItemVm>
                {
                    new("home","Tổng quan"),
                    new("courses","Khóa học"),
                    new("registrations","Đăng ký của tôi"),
                    new("schedule","Lịch học"),
                    new("profile","Hồ sơ")
                }
            };
        }

        private UserControl ResolvePage(string key, bool forceReload = false)
        {
            if (_currentUser == null) return new LoginView(OnLoginSuccess);

            if (forceReload && _cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }

            if (_cache.TryGetValue(key, out var page)) return page;

            page = key switch
            {
                "home" => new DashboardView(_currentUser),
                "courses" => _currentUser.Role == "Admin"
                    ? new AdminCoursesView()
                    : new StudentCoursesView(_currentUser),
                "classes" => new AdminClassesView(),
                "registrations" => _currentUser.Role == "Admin"
                    ? new AdminRegistrationsView()
                    : new StudentEnrollmentsView(_currentUser),
                "people" => new AdminPeopleView(),
                "schedule" => new ScheduleView(_currentUser),
                "profile" => new ProfileView(_currentUser, RefreshUserInfo),
                _ => new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Page not found",
                        FontSize = 24,
                        Margin = new Thickness(20)
                    }
                }
            };

            _cache[key] = page;
            return page;
        }

        public void NavigateTo(string key, bool forceReload = false)
        {
            if (NavListBox.ItemsSource is not IEnumerable<MenuItemVm> items) return;

            var target = items.FirstOrDefault(x => x.Key == key);
            if (target == null) return;

            MainContent.Content = ResolvePage(key, forceReload);
            NavListBox.SelectedItem = target;
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is MenuItemVm vm)
            {
                MainContent.Content = ResolvePage(vm.Key);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e) => ShowLogin();
    }

    public record MenuItemVm(string Key, string Title);
}