using System.Windows;
using WpfApp.Views.Admin;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var window = new  RegistrationListWindow();
            Application.Current.MainWindow = window;
            window.Show();
            this.Close();
        }
    }
}