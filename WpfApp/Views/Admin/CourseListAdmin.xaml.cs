using BusinessObjects;
using DataAccess;
using Repositories;
using Services;
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

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for Window1_Course_.xaml
    /// </summary>
    public partial class Window1_Course_ : Window
    {
        private readonly ICourseService _service;
        private Course? selectedCourse = null;
        public Window1_Course_()
        {
            InitializeComponent();

            var context = new LctmsDbContext();
            var repo = new CourseRepository(context);
            _service = new CourseService(repo);

            LoadData();
        }
        private void LoadData()
        {
            dgCourses.ItemsSource = _service.GetAllCourses();
        }

        // ADD
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddCourseWindow();
            win.ShowDialog();

            LoadData();
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCourses.SelectedItem is Course course)
            {
                var win = new UpdateCourseWindow(course);
                win.ShowDialog();
                LoadData();
            }
            else
            {
                MessageBox.Show("Chọn course trước!");
            }
        }
    }
}
