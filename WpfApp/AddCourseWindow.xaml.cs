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
using System.Xml.Linq;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for AddCourseWindow.xaml
    /// </summary>
    public partial class AddCourseWindow : Window
    {
        private readonly ICourseService _service;
        public AddCourseWindow()
        {
            InitializeComponent();

            var context = new LctmsDbContext();
            var repo = new CourseRepository(context);
            _service = new CourseService(repo);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var course = new Course
                {
                    CourseCode = txtCourseCode.Text,
                    Name = txtName.Text,
                    SubjectCourse = (cbSubject.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    DurationWeeks = int.Parse(txtDuration.Text),
                    Fee = decimal.Parse(txtFee.Text),
                    Status = rbOpen.IsChecked == true ? "Open" : "Closed",
                    Description = txtDescription.Text
                };

                _service.AddCourse(course);

                MessageBox.Show("Thêm khóa học thành công!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
