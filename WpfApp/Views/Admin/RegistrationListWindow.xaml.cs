using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services;

namespace WpfApp.Views.Admin
{
    public partial class RegistrationListWindow : Window
    {
        private readonly IEnrollmentService _enrollmentService;

        public RegistrationListWindow()
        {
            InitializeComponent();
            _enrollmentService = new EnrollmentService();
            LoadData();
        }

        private void LoadData()
        {
            string status = (cbStatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All";
            string keyword = txtKeyword.Text.Trim();

            dgRegistrations.ItemsSource = _enrollmentService.GetRegistrationList(status, keyword);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Text = string.Empty;
            cbStatusFilter.SelectedIndex = 0;
            LoadData();
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EnrollmentApprovalItem item)
            {
                var confirm = MessageBox.Show(
                    $"Approve enrollment of {item.StudentName} in class {item.ClassCode}?",
                    "Confirm Approve",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                var result = _enrollmentService.ApproveEnrollment(item.EnrollmentId);

                MessageBox.Show(
                    result.Message,
                    result.IsSuccess ? "Success" : "Error",
                    MessageBoxButton.OK,
                    result.IsSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);

                LoadData();
            }
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EnrollmentApprovalItem item)
            {
                var confirm = MessageBox.Show(
                    $"Reject enrollment of {item.StudentName} in class {item.ClassCode}?",
                    "Confirm Reject",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                    return;

                var result = _enrollmentService.RejectEnrollment(item.EnrollmentId);

                MessageBox.Show(
                    result.Message,
                    result.IsSuccess ? "Success" : "Error",
                    MessageBoxButton.OK,
                    result.IsSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);

                LoadData();
            }
        }
    }
}