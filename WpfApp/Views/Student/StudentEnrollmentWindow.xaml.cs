using BusinessObjects;
using Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.View
{
    public partial class StudentEnrollmentWindow : Window
    {
        private readonly IStudentCourseService _service;

        public StudentEnrollmentWindow()
        {
            InitializeComponent();
            _service = new StudentCourseService();

            LoadData();
        }

        private void LoadData()
        {
            int studentId = 1; // TODO: lấy từ session

            dgEnrollments.ItemsSource = _service.GetStudentEnrollments(studentId);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StudentEnrollmentDto item)
            {
                int studentId = 1;

                try
                {
                    _service.CancelEnrollment(studentId, item.ClassId);

                    MessageBox.Show("Đã hủy đăng ký!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}