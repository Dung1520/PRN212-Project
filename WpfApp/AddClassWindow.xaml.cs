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
    /// Interaction logic for AddClassWindow.xaml
    /// </summary>
    public partial class AddClassWindow : Window
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IScheduleService _scheduleService;
        private readonly ISlotService _slotService;
        private List<Schedule> tempSchedules = new();

        public AddClassWindow()
        {
            InitializeComponent();

            var context = new LctmsDbContext();

            _classService = new ClassService(new ClassRepository(context),context);
            _courseService = new CourseService(new CourseRepository(context));
            _teacherService = new TeacherService(new TeacherRepository(context));
            _scheduleService = new ScheduleService(new ScheduleRepository(context));
            _slotService = new SlotService(new SlotRepository(context));

            LoadData();
        }

        private void LoadData()
        {
            cbCourse.ItemsSource = _courseService.GetAllCourses();
            cbTeacher.ItemsSource = _teacherService.GetAllTeachers();
            cbSlot.ItemsSource = _slotService.GetAllSlots();

            // Room: Phòng 01 → 30
            cbRoom.ItemsSource = Enumerable.Range(1, 30)
                .Select(i => $"Room {i:D2}")
                .ToList();
        }

        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (cbDay.SelectedItem is ComboBoxItem dayItem)
            {
                byte day = byte.Parse(dayItem.Tag.ToString());

                if (cbSlot.SelectedValue == null)
                {
                    MessageBox.Show("Chọn Slot!");
                    return;
                }

                if (cbRoom.SelectedItem == null)
                {
                    MessageBox.Show("Chọn phòng!");
                    return;
                }

                var schedule = new Schedule
                {
                    DayOfWeek = day,
                    SlotId = (int)cbSlot.SelectedValue,
                    RoomName = cbRoom.SelectedItem?.ToString()
                };

                // ❌ tránh trùng
                if (tempSchedules.Any(s =>
                    s.DayOfWeek == schedule.DayOfWeek &&
                    s.SlotId == schedule.SlotId))
                {
                    MessageBox.Show("Trùng lịch!");
                    return;
                }

                tempSchedules.Add(schedule);

                dgSchedules.ItemsSource = tempSchedules.ToList();
            }
        }

        private void RemoveSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (dgSchedules.SelectedItem is Schedule s)
            {
                tempSchedules.Remove(s);

                dgSchedules.ItemsSource = tempSchedules.ToList();
            }
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtClassCode.Text))
                {
                    MessageBox.Show("Nhập ClassCode!");
                    return;
                }

                if (!int.TryParse(txtCapacity.Text, out int capacity))
                {
                    MessageBox.Show("Capacity phải là số!");
                    return;
                }

                if (cbCourse.SelectedValue == null)
                {
                    MessageBox.Show("Chọn Course!");
                    return;
                }

                if (tempSchedules.Count == 0)
                {
                    MessageBox.Show("Phải có ít nhất 1 lịch học!");
                    return;
                }

                // 1. tạo class
                var newClass = new Class
                {
                    ClassCode = txtClassCode.Text,
                    Capacity = capacity,
                    CourseId = (int)cbCourse.SelectedValue,
                    TeacherId = cbTeacher.SelectedValue != null
                                ? (int)cbTeacher.SelectedValue
                                : null,
                    StartDate = dpStartDate.SelectedDate ?? DateTime.Now,
                    EndDate = dpEndDate.SelectedDate ?? DateTime.Now,
                    Status = "Open"
                };

                _classService.AddClass(newClass, tempSchedules);

                MessageBox.Show("Tạo lớp + lịch thành công!");
                this.Close();
            }
            catch (Exception ex)
            {
                // Show inner / root exception and stack trace to diagnose DB errors
                var baseEx = ex.GetBaseException();
                MessageBox.Show(
                    "Error saving changes:\n" +
                    $"{ex.Message}\n\nInner: {baseEx.Message}\n\nStackTrace:\n{ex.StackTrace}",
                    "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
