using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace WpfApp.Views.Admin
{
    public partial class StudentListWindow : Window
    {
        private readonly IStudentService _studentService;

        public StudentListWindow()
        {
            InitializeComponent();
            _studentService = new StudentService();
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtKeyword.Text.Trim();
            dgStudents.ItemsSource = _studentService.GetStudentList(keyword);
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
            if (sender is Button button && button.DataContext is StudentListItem item)
            {
                var detailWindow = new StudentDetailWindow(item.Id);
                detailWindow.ShowDialog();
            }
        }
    }
}