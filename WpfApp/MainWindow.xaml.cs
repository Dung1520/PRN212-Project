using BusinessObjects;
using System.Windows;
using WpfApp.View;
using WpfApp.Views.Admin;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var window = new StudentCourseWindow();
            Application.Current.MainWindow = window;
            window.Show();
            this.Close();
        }
  }
}