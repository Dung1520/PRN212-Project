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
    /// Interaction logic for AdminScheduleDetailWindow.xaml
    /// </summary>
    public partial class AdminScheduleDetailWindow : Window
    {
        public AdminScheduleDetailWindow(AdminScheduleDetailViewModel data)
        {
            InitializeComponent();

            txtClassInfo.Text = $"{data.ClassCode} - {data.CourseName}";
            txtTeacher.Text = $"Teacher: {data.TeacherName}";
            txtRoom.Text = $"Room: {data.RoomName}";
            txtTime.Text = $"Time: {data.SlotName} ({data.StartTime:hh\\:mm}-{data.EndTime:hh\\:mm}) | {data.StartDate:dd/MM/yyyy} - {data.EndDate:dd/MM/yyyy}";
            txtStatus.Text = $"Status: {data.Status} | Capacity: {data.Capacity}";
            lstStudents.ItemsSource = data.StudentNames;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
