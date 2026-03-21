using System.Windows;

namespace WpfApp.Views.Admin
{
    public partial class PeopleManagementWindow : Window
    {
        public PeopleManagementWindow()
        {
            InitializeComponent();
        }

        private void BtnStudentList_Click(object sender, RoutedEventArgs e)
        {
            var window = new StudentListWindow();
            window.Show();
            Close();
        }

        private void BtnTeacherList_Click(object sender, RoutedEventArgs e)
        {
            var window = new TeacherListWindow();
            window.Show();
            Close();
        }
    }
}