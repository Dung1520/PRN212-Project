using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for TeacherScheduleDetailWindow.xaml
    /// </summary>
    public partial class TeacherScheduleDetailWindow : Window
    {
        public TeacherScheduleDetailWindow(TeacherScheduleDetailViewModel data)
        {
            InitializeComponent();

            txtClassInfo.Text = $"{data.ClassCode} - {data.CourseName}";
            txtRoom.Text = $"Room: {data.RoomName}";
            txtTime.Text = $"Time: {data.SlotName} ({data.StartTime:hh\\:mm}-{data.EndTime:hh\\:mm}) | {data.StartDate:dd/MM/yyyy} - {data.EndDate:dd/MM/yyyy}";
            lstStudents.ItemsSource = data.StudentNames;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
