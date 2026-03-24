using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp.View;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StudentCourseWindow window = new StudentCourseWindow();
          //  StudentEnrollmentWindow window = new StudentEnrollmentWindow();
            window.Show();
            this.Close();
        }
    }
}