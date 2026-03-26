using BusinessObjects;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp.Views
{
    public partial class ScheduleView : UserControl
    {
        private readonly LoginUser _user;
        private readonly IScheduleService _service = new ScheduleService();
        private DateTime _currentDate = DateTime.Today;
        private ScheduleWeekViewModel _currentWeek;

        public ScheduleView(LoginUser user)
        {
            InitializeComponent();
            _user = user;
            ConfigureFilterByRole();
            LoadFilterOptions();
            LoadSchedule();
        }

        private void ConfigureFilterByRole()
        {
            if (_user.Role == "Student")
            {
                TeacherFilterComboBox.Visibility = Visibility.Collapsed;
                CourseFilterComboBox.Visibility = Visibility.Collapsed;
                ClassFilterComboBox.Visibility = Visibility.Collapsed;
                SlotFilterComboBox.Visibility = Visibility.Collapsed;
            }
            else if (_user.Role == "Teacher")
            {
                TeacherFilterComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadFilterOptions()
        {
            if (_user.Role == "Student")
                return;

            var options = _service.GetScheduleFilterOptions(_user.UserId, _user.Role, _currentDate);

            TeacherFilterComboBox.ItemsSource = options.TeacherOptions;
            CourseFilterComboBox.ItemsSource = options.CourseOptions;
            ClassFilterComboBox.ItemsSource = options.ClassOptions;
            SlotFilterComboBox.ItemsSource = options.SlotOptions;

            TeacherFilterComboBox.SelectedIndex = -1;
            CourseFilterComboBox.SelectedIndex = -1;
            ClassFilterComboBox.SelectedIndex = -1;
            SlotFilterComboBox.SelectedIndex = -1;
        }

        private int? GetSelectedInt(ComboBox comboBox)
        {
            if (comboBox.SelectedValue == null)
                return null;

            if (comboBox.SelectedValue is int)
                return (int)comboBox.SelectedValue;

            int parsed;
            if (int.TryParse(comboBox.SelectedValue.ToString(), out parsed))
                return parsed;

            return null;
        }

        private ScheduleFilterViewModel BuildFilter()
        {
            if (_user.Role == "Student")
                return null;

            return new ScheduleFilterViewModel
            {
                Keyword = string.IsNullOrWhiteSpace(KeywordTextBox.Text) ? null : KeywordTextBox.Text.Trim(),
                TeacherId = GetSelectedInt(TeacherFilterComboBox),
                CourseId = GetSelectedInt(CourseFilterComboBox),
                ClassId = GetSelectedInt(ClassFilterComboBox),
                SlotId = GetSelectedInt(SlotFilterComboBox)
            };
        }

        private void LoadSchedule()
        {
            _currentWeek = _service.GetWeeklySchedule(_user.UserId, _user.Role, _currentDate, BuildFilter());
            WeekText.Text = string.Format("{0:dd/MM/yyyy} - {1:dd/MM/yyyy}", _currentWeek.WeekStartDate, _currentWeek.WeekEndDate);
            RenderGrid(_currentWeek);
        }

        private void RenderGrid(ScheduleWeekViewModel week)
        {
            ScheduleGrid.Children.Clear();
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();

            var slots = week.Cells
                .Select(x => new SlotHeaderVm
                {
                    SlotId = x.SlotId,
                    SlotName = x.SlotName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime
                })
                .GroupBy(x => x.SlotId)
                .Select(g => g.First())
                .OrderBy(x => x.SlotId)
                .ToList();

            ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            for (int d = 1; d <= 7; d++)
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });

            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < slots.Count; i++)
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddHeader(0, 0, "Slot / Day");

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int d = 0; d < 7; d++)
                AddHeader(0, d + 1, days[d]);

            for (int r = 0; r < slots.Count; r++)
            {
                var slot = slots[r];
                AddTextCell(r + 1, 0,
                    string.Format("{0}\n{1:hh\\:mm}-{2:hh\\:mm}", slot.SlotName, slot.StartTime, slot.EndTime),
                    true);

                for (int d = 1; d <= 7; d++)
                {
                    var matchedCells = week.Cells
                        .Where(x => x.SlotId == slot.SlotId && x.DayOfWeek == d && x.HasClass)
                        .OrderBy(x => x.ClassCode)
                        .ToList();

                    if (matchedCells.Count > 0)
                        AddScheduleStackCell(r + 1, d, matchedCells);
                    else
                        AddTextCell(r + 1, d, string.Empty, false);
                }
            }
        }

        private void AddHeader(int row, int col, string text)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromRgb(239, 246, 255)),
                Padding = new Thickness(8)
            };

            border.Child = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            ScheduleGrid.Children.Add(border);
        }

        private void AddTextCell(int row, int col, string text, bool headerCol)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Background = headerCol ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : Brushes.White
            };

            border.Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            ScheduleGrid.Children.Add(border);
        }

        private void AddScheduleStackCell(int row, int col, List<ScheduleCellViewModel> cells)
        {
            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                Background = Brushes.White
            };

            var panel = new StackPanel();

            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                var button = new Button
                {
                    Content = new TextBlock
                    {
                        Text = string.Format("{0}\n{1}\n{2}", cell.ClassCode, cell.CourseName, cell.RoomName),
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    },
                    Tag = cell,
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 0, 0, i == cells.Count - 1 ? 0 : 4),
                    Background = new SolidColorBrush(Color.FromRgb(219, 234, 254)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(96, 165, 250))
                };

                button.Click += ScheduleCell_Click;
                panel.Children.Add(button);
            }

            border.Child = panel;

            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            ScheduleGrid.Children.Add(border);
        }

        private void ScheduleCell_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            ScheduleCellViewModel cell = btn.Tag as ScheduleCellViewModel;
            if (cell == null || !cell.ClassId.HasValue)
                return;

            if (_user.Role == "Admin")
            {
                var detail = _service.GetAdminScheduleDetail(cell.ClassId.Value, cell.DayOfWeek, cell.SlotId);
                if (detail == null)
                    return;

                DetailTitleText.Text = "Lớp: " + detail.ClassCode;
                DetailCourseText.Text = "Khóa học: " + detail.CourseCode + " - " + detail.CourseName;
                DetailTeacherText.Text = "Giáo viên: " + detail.TeacherName;
                DetailRoomText.Text = "Phòng: " + detail.RoomName;
                DetailSlotText.Text = string.Format("Slot: {0} ({1:hh\\:mm} - {2:hh\\:mm})", detail.SlotName, detail.StartTime, detail.EndTime);
                DetailDateText.Text = string.Format("Thời gian lớp: {0:dd/MM/yyyy} - {1:dd/MM/yyyy}", detail.StartDate, detail.EndDate);
                DetailCapacityStatusText.Text = "Sức chứa/Trạng thái: " + detail.Capacity + " / " + detail.Status;
                StudentListBox.ItemsSource = detail.StudentNames;
                return;
            }

            if (_user.Role == "Teacher")
            {
                var detail = _service.GetTeacherScheduleDetail(_user.UserId, cell.ClassId.Value, cell.DayOfWeek, cell.SlotId);
                if (detail == null)
                    return;

                DetailTitleText.Text = "Lớp: " + detail.ClassCode;
                DetailCourseText.Text = "Khóa học: " + detail.CourseCode + " - " + detail.CourseName;
                DetailTeacherText.Text = "Giáo viên: chính bạn";
                DetailRoomText.Text = "Phòng: " + detail.RoomName;
                DetailSlotText.Text = string.Format("Slot: {0} ({1:hh\\:mm} - {2:hh\\:mm})", detail.SlotName, detail.StartTime, detail.EndTime);
                DetailDateText.Text = string.Format("Thời gian lớp: {0:dd/MM/yyyy} - {1:dd/MM/yyyy}", detail.StartDate, detail.EndDate);
                DetailCapacityStatusText.Text = "Sức chứa/Trạng thái: " + detail.Capacity + " / " + detail.Status;
                StudentListBox.ItemsSource = detail.StudentNames;
                return;
            }

            var studentDetail = _service.GetStudentScheduleDetail(_user.UserId, cell.ClassId.Value, cell.DayOfWeek, cell.SlotId);
            if (studentDetail == null)
                return;

            DetailTitleText.Text = "Lớp: " + studentDetail.ClassCode;
            DetailCourseText.Text = "Khóa học: " + studentDetail.CourseCode + " - " + studentDetail.CourseName;
            DetailTeacherText.Text = "Giáo viên: " + studentDetail.TeacherName;
            DetailRoomText.Text = "Phòng: " + studentDetail.RoomName;
            DetailSlotText.Text = string.Format("Slot: {0} ({1:hh\\:mm} - {2:hh\\:mm})", studentDetail.SlotName, studentDetail.StartTime, studentDetail.EndTime);
            DetailDateText.Text = string.Format("Thời gian lớp: {0:dd/MM/yyyy} - {1:dd/MM/yyyy}", studentDetail.StartDate, studentDetail.EndDate);
            DetailCapacityStatusText.Text = "Sức chứa/Trạng thái: " + studentDetail.Capacity + " / " + studentDetail.Status;
            StudentListBox.ItemsSource = studentDetail.StudentNames;
        }

        private void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(-7);
            LoadFilterOptions();
            LoadSchedule();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(7);
            LoadFilterOptions();
            LoadSchedule();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            KeywordTextBox.Text = string.Empty;
            TeacherFilterComboBox.SelectedIndex = -1;
            CourseFilterComboBox.SelectedIndex = -1;
            ClassFilterComboBox.SelectedIndex = -1;
            SlotFilterComboBox.SelectedIndex = -1;
            LoadSchedule();
        }

        private class SlotHeaderVm
        {
            public int SlotId { get; set; }
            public string SlotName { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
    }
}