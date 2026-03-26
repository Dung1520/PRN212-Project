using BusinessObjects;
using DataAccess;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp.Views
{
    public partial class AdminCoursesView : UserControl
    {
        private readonly ICourseService _service;
        private List<Course> _items = new List<Course>();
        private int _selectedId;

        public AdminCoursesView()
        {
            InitializeComponent();
            var context = DbContextFactory.CreateDbContext();
            _service = new CourseService(new CourseRepository(context));
            LoadData();
            HideForm();
        }

        private void LoadData()
        {
            _items = _service.GetAllCourses().OrderBy(x => x.CourseCode).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = SearchTextBox.Text == null ? string.Empty : SearchTextBox.Text.Trim();

            CourseGrid.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? _items
                : _items.Where(x =>
                    x.CourseCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(x.SubjectCourse) && x.SubjectCourse.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private void ShowForm(string modeText)
        {
            ActionHintPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;
            ModeText.Text = modeText;
        }

        private void HideForm()
        {
            ActionHintPanel.Visibility = Visibility.Visible;
            FormPanel.Visibility = Visibility.Collapsed;
            ModeText.Text = "Chưa chọn thao tác";
            MessageText.Text = string.Empty;
        }

        private Course ReadForm()
        {
            int durationWeeks;
            decimal fee;

            if (!int.TryParse(DurationTextBox.Text.Trim(), out durationWeeks))
                throw new Exception("Số tuần phải là số nguyên.");

            if (!decimal.TryParse(FeeTextBox.Text.Trim(), out fee))
                throw new Exception("Học phí không hợp lệ.");

            return new Course
            {
                Id = _selectedId,
                CourseCode = CourseCodeTextBox.Text.Trim(),
                Name = NameTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                SubjectCourse = SubjectTextBox.Text.Trim(),
                DurationWeeks = durationWeeks,
                Fee = fee,
                Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content == null
                    ? "Open"
                    : ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString()
            };
        }

        private void FillForm(Course c)
        {
            _selectedId = c.Id;
            CourseCodeTextBox.Text = c.CourseCode;
            NameTextBox.Text = c.Name;
            DescriptionTextBox.Text = c.Description;
            SubjectTextBox.Text = c.SubjectCourse;
            DurationTextBox.Text = c.DurationWeeks.ToString();
            FeeTextBox.Text = c.Fee.ToString("0.##");
            StatusComboBox.SelectedIndex = c.Status == "Closed" ? 1 : 0;
            ShowForm("Mode: Edit");
        }

        private void ClearForm()
        {
            _selectedId = 0;
            CourseCodeTextBox.Text = string.Empty;
            NameTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            SubjectTextBox.Text = string.Empty;
            DurationTextBox.Text = string.Empty;
            FeeTextBox.Text = string.Empty;
            StatusComboBox.SelectedIndex = 0;
            MessageText.Text = string.Empty;
            CourseGrid.SelectedItem = null;
        }

        private Course GetCourseByIdFromCurrentList(int id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ShowForm("Mode: Add");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(((Button)sender).Tag == null ? "0" : ((Button)sender).Tag.ToString());
            var course = GetCourseByIdFromCurrentList(id);
            if (course != null)
            {
                FillForm(course);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            HideForm();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Execute(() =>
            {
                var model = ReadForm();
                if (_selectedId > 0)
                    _service.UpdateCourse(model);
                else
                    _service.AddCourse(model);
            }, _selectedId > 0 ? "Cập nhật khóa học thành công." : "Thêm khóa học thành công.");
        }

        private void Execute(Action action, string okMessage)
        {
            try
            {
                action();
                MessageText.Foreground = Brushes.Green;
                MessageText.Text = okMessage;
                LoadData();
                ClearForm();
                HideForm();
            }
            catch (Exception ex)
            {
                MessageText.Foreground = Brushes.Crimson;
                MessageText.Text = ex.Message;
            }
        }
    }
}