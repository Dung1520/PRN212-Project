using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace WpfApp.Views.Admin
{
    public partial class TeacherListWindow : Window
    {
        private readonly ITeacherService _teacherService;

        public TeacherListWindow()
        {
            InitializeComponent();
            _teacherService = new TeacherService();
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtKeyword.Text.Trim();
            dgTeachers.ItemsSource = _teacherService.GetTeacherList(keyword);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Text = string.Empty;
            LoadData();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var window = new PeopleManagementWindow();
            window.Show();
            Close();
        }

        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TeacherListItem item)
            {
                var detailWindow = new TeacherDetailWindow(item.Id);
                detailWindow.ShowDialog();
            }
        }
    }
}