using BusinessObjects;
using System.Windows;
using WpfApp.View;
using WpfApp.Views;
using WpfApp.Views.Admin;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var window = new LoginWindow();
            Application.Current.MainWindow = window;
            window.Show();
            this.Close();
        }
  }
}