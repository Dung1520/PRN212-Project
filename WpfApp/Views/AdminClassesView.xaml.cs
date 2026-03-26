using BusinessObjects;
using DataAccess;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp.Views
{
    public partial class AdminClassesView : UserControl
    {
        private readonly LctmsDbContext _context;
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly ISlotService _slotService;
        private readonly IScheduleService _scheduleService;

        private List<Class> _items = new List<Class>();
        private ObservableCollection<ScheduleRowVm> _scheduleRows = new ObservableCollection<ScheduleRowVm>();
        private int _selectedId;

        public List<DayOptionItem> DayOptions { get; set; }
        public List<Slot> SlotOptions { get; set; }

        public AdminClassesView()
        {
            InitializeComponent();
            DataContext = this;

            _context = DbContextFactory.CreateDbContext();
            _classService = new ClassService(new ClassRepository(_context), _context);
            _courseService = new CourseService(new CourseRepository(_context));
            _teacherService = new TeacherService(new TeacherRepository(_context));
            _slotService = new SlotService(new SlotRepository(_context));
            _scheduleService = new ScheduleService(new ScheduleRepository(_context));

            DayOptions = new List<DayOptionItem>
            {
                new DayOptionItem { Value = 1, Text = "Monday" },
                new DayOptionItem { Value = 2, Text = "Tuesday" },
                new DayOptionItem { Value = 3, Text = "Wednesday" },
                new DayOptionItem { Value = 4, Text = "Thursday" },
                new DayOptionItem { Value = 5, Text = "Friday" },
                new DayOptionItem { Value = 6, Text = "Saturday" },
                new DayOptionItem { Value = 7, Text = "Sunday" }
            };

            SlotOptions = _slotService.GetAllSlots().OrderBy(x => x.Id).ToList();

            CourseComboBox.ItemsSource = _courseService.GetAllCourses().OrderBy(x => x.Name).ToList();
            TeacherComboBox.ItemsSource = _teacherService.GetAllTeachers().OrderBy(x => x.FullName).ToList();
            ScheduleGrid.ItemsSource = _scheduleRows;

            AddDefaultScheduleRow();
            LoadClasses();
            HideForm();
        }

        private void LoadClasses()
        {
            _items = _classService.GetAllClasses().OrderBy(x => x.ClassCode).ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string keyword = SearchTextBox.Text == null ? string.Empty : SearchTextBox.Text.Trim();

            ClassGrid.ItemsSource = string.IsNullOrWhiteSpace(keyword)
                ? _items
                : _items.Where(x => x.ClassCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
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

        private void AddDefaultScheduleRow()
        {
            if (SlotOptions == null || SlotOptions.Count == 0)
                return;

            _scheduleRows.Add(new ScheduleRowVm
            {
                DayOfWeek = 1,
                SlotId = SlotOptions[0].Id,
                RoomName = "Room A"
            });
        }

        private void RecalculateEndDate()
        {
            if (StartDatePicker.SelectedDate == null || CourseComboBox.SelectedValue == null)
            {
                EndDateTextBox.Text = string.Empty;
                return;
            }

            int courseId = (int)CourseComboBox.SelectedValue;
            var course = _courseService.GetCourseById(courseId);
            if (course == null)
            {
                EndDateTextBox.Text = string.Empty;
                return;
            }

            DateTime endDate = StartDatePicker.SelectedDate.Value.Date.AddDays(course.DurationWeeks * 7 - 1);
            EndDateTextBox.Text = endDate.ToString("yyyy-MM-dd");
        }

        private Class ReadClass()
        {
            if (StartDatePicker.SelectedDate == null)
                throw new Exception("Bạn phải chọn ngày bắt đầu.");

            int capacity;
            if (!int.TryParse(CapacityTextBox.Text.Trim(), out capacity))
                throw new Exception("Sức chứa phải là số nguyên.");

            if (CourseComboBox.SelectedValue == null)
                throw new Exception("Bạn phải chọn khóa học.");

            return new Class
            {
                Id = _selectedId,
                ClassCode = ClassCodeTextBox.Text.Trim(),
                CourseId = (int)CourseComboBox.SelectedValue,
                TeacherId = TeacherComboBox.SelectedValue == null ? null : (int?)Convert.ToInt32(TeacherComboBox.SelectedValue),
                StartDate = StartDatePicker.SelectedDate.Value.Date,
                EndDate = string.IsNullOrWhiteSpace(EndDateTextBox.Text) ? DateTime.MinValue : DateTime.Parse(EndDateTextBox.Text),
                Capacity = capacity,
                Status = ((ComboBoxItem)ClassStatusComboBox.SelectedItem).Content.ToString()
            };
        }

        private List<Schedule> ReadSchedules()
        {
            var result = new List<Schedule>();

            if (_scheduleRows == null || _scheduleRows.Count == 0)
                throw new Exception("Bạn phải nhập ít nhất một dòng lịch học.");

            foreach (var x in _scheduleRows)
            {
                result.Add(new Schedule
                {
                    DayOfWeek = (byte)x.DayOfWeek,
                    SlotId = x.SlotId,
                    RoomName = x.RoomName == null ? string.Empty : x.RoomName.Trim()
                });
            }

            return result;
        }

        private void Execute(Action action, string success)
        {
            try
            {
                action();
                MessageText.Foreground = Brushes.Green;
                MessageText.Text = success;
                LoadClasses();
                ClearForm();
                HideForm();
            }
            catch (Exception ex)
            {
                MessageText.Foreground = Brushes.Crimson;
                MessageText.Text = ex.Message;
            }
        }

        private void ClearForm()
        {
            _selectedId = 0;
            ClassCodeTextBox.Text = string.Empty;
            CapacityTextBox.Text = string.Empty;
            StartDatePicker.SelectedDate = null;
            EndDateTextBox.Text = string.Empty;
            CourseComboBox.SelectedIndex = -1;
            TeacherComboBox.SelectedIndex = -1;
            ClassStatusComboBox.SelectedIndex = 0;
            MessageText.Text = string.Empty;

            _scheduleRows = new ObservableCollection<ScheduleRowVm>();
            ScheduleGrid.ItemsSource = _scheduleRows;
            AddDefaultScheduleRow();

            ClassGrid.SelectedItem = null;
        }

        private void FillForm(Class item)
        {
            _selectedId = item.Id;
            ClassCodeTextBox.Text = item.ClassCode;
            CourseComboBox.SelectedValue = item.CourseId;
            TeacherComboBox.SelectedValue = item.TeacherId;
            StartDatePicker.SelectedDate = item.StartDate;
            EndDateTextBox.Text = item.EndDate.ToString("yyyy-MM-dd");
            CapacityTextBox.Text = item.Capacity.ToString();

            if (item.Status == "Full")
                ClassStatusComboBox.SelectedIndex = 1;
            else if (item.Status == "Closed")
                ClassStatusComboBox.SelectedIndex = 2;
            else
                ClassStatusComboBox.SelectedIndex = 0;

            var oldSchedules = _scheduleService.GetSchedulesByClassId(item.Id);

            _scheduleRows = new ObservableCollection<ScheduleRowVm>(
                oldSchedules.Select(s => new ScheduleRowVm
                {
                    DayOfWeek = s.DayOfWeek,
                    SlotId = s.SlotId,
                    RoomName = s.RoomName == null ? string.Empty : s.RoomName
                }).ToList());

            if (_scheduleRows.Count == 0)
                AddDefaultScheduleRow();

            ScheduleGrid.ItemsSource = _scheduleRows;
            ShowForm("Mode: Edit");
        }

        private Class GetClassByIdFromCurrentList(int id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ShowForm("Mode: Add");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(((Button)sender).Tag == null ? "0" : ((Button)sender).Tag.ToString());
            var selectedClass = GetClassByIdFromCurrentList(id);
            if (selectedClass != null)
            {
                FillForm(selectedClass);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            HideForm();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void AddScheduleRow_Click(object sender, RoutedEventArgs e)
        {
            AddDefaultScheduleRow();
        }

        private void RemoveScheduleRow_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleGrid.SelectedItem is ScheduleRowVm)
            {
                _scheduleRows.Remove((ScheduleRowVm)ScheduleGrid.SelectedItem);
            }
            else if (_scheduleRows.Count > 0)
            {
                _scheduleRows.RemoveAt(_scheduleRows.Count - 1);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Execute(() =>
            {
                var classModel = ReadClass();
                var schedules = ReadSchedules();

                if (_selectedId > 0)
                    _classService.UpdateClass(classModel, schedules);
                else
                    _classService.AddClass(classModel, schedules);
            }, _selectedId > 0 ? "Cập nhật lớp thành công." : "Thêm lớp thành công.");
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateEndDate();
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculateEndDate();
        }
    }

    public class DayOptionItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class ScheduleRowVm : INotifyPropertyChanged
    {
        private int _dayOfWeek;
        private int _slotId;
        private string _roomName;

        public int DayOfWeek
        {
            get { return _dayOfWeek; }
            set
            {
                _dayOfWeek = value;
                OnPropertyChanged("DayOfWeek");
            }
        }

        public int SlotId
        {
            get { return _slotId; }
            set
            {
                _slotId = value;
                OnPropertyChanged("SlotId");
            }
        }

        public string RoomName
        {
            get { return _roomName; }
            set
            {
                _roomName = value;
                OnPropertyChanged("RoomName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}