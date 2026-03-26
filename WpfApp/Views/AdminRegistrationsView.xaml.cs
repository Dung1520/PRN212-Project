using Services;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Views
{
    public partial class AdminRegistrationsView : UserControl
    {
        private readonly IEnrollmentService _service = new EnrollmentService();

        public AdminRegistrationsView()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content?.ToString();
            RegistrationGrid.ItemsSource = _service.GetRegistrationList(status, KeywordTextBox.Text.Trim());
        }

        private void Search_Click(object sender, RoutedEventArgs e) => LoadData();
        private void Reload_Click(object sender, RoutedEventArgs e) { KeywordTextBox.Text = string.Empty; StatusComboBox.SelectedIndex = 0; LoadData(); }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            var id = int.Parse(((Button)sender).Tag.ToString()!);
            var result = _service.ApproveEnrollment(id);
            MessageBox.Show(result.Message);
            LoadData();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            var id = int.Parse(((Button)sender).Tag.ToString()!);
            var result = _service.RejectEnrollment(id);
            MessageBox.Show(result.Message);
            LoadData();
        }
    }
}