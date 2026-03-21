using System.Windows;
using WpfApp.Views.Admin;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //var window = new RegistrationListWindow();
            //Application.Current.MainWindow = window;
            //window.Show();
            //this.Close();
        }

    private void Course_Click(object sender, RoutedEventArgs e)
        {
            new Window1_Course_().Show();
        }
        private void Class_Click(object sender, RoutedEventArgs e)
        {
            new Window2_Class_().Show();
        }
    }
}