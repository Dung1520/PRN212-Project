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
    /// Interaction logic for UpdateCourseWindow.xaml
    /// </summary>
    public partial class UpdateCourseWindow : Window
    {
        private readonly ICourseService _service;
        private Course _course;

        public UpdateCourseWindow(Course course)
        {
            InitializeComponent();

            var context = new LctmsDbContext();
            var repo = new CourseRepository(context);
            _service = new CourseService(repo);

            _course = course;

            LoadCourse();
        }

        private void LoadCourse()
        {
            txtCourseCode.Text = _course.CourseCode;
            txtCourseCode.IsEnabled = false; // ❗ không cho sửa code

            txtName.Text = _course.Name;
            txtDuration.Text = _course.DurationWeeks.ToString();
            txtFee.Text = _course.Fee.ToString();
            txtDescription.Text = _course.Description;

            // Subject
            foreach (ComboBoxItem item in cbSubject.Items)
            {
                if (item.Content.ToString() == _course.SubjectCourse)
                {
                    cbSubject.SelectedItem = item;
                    break;
                }
            }

            // Status
            if (_course.Status == "Open")
                rbOpen.IsChecked = true;
            else
                rbClosed.IsChecked = true;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtDuration.Text, out int duration))
                {
                    MessageBox.Show("Duration phải là số!");
                    return;
                }

                if (!decimal.TryParse(txtFee.Text, out decimal fee))
                {
                    MessageBox.Show("Fee phải là số!");
                    return;
                }

                _course.Name = txtName.Text;
                _course.SubjectCourse = (cbSubject.SelectedItem as ComboBoxItem)?.Content.ToString();
                _course.DurationWeeks = duration;
                _course.Fee = fee;
                _course.Status = rbOpen.IsChecked == true ? "Open" : "Closed";
                _course.Description = txtDescription.Text;

                _service.UpdateCourse(_course);

                MessageBox.Show("Cập nhật thành công!");
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