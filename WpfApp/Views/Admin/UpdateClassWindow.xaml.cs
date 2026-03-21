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
    /// Interaction logic for UpdateClassWindow.xaml
    /// </summary>
    public partial class UpdateClassWindow : Window
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly ITeacherService _teacherService;
        private readonly IScheduleService _scheduleService;
        private readonly ISlotService _slotService;
        private List<Schedule> tempSchedules = new();
        private Class _currentClass;
        public UpdateClassWindow(Class selectedClass)
        {
            InitializeComponent();

            var context = new LctmsDbContext();

            _classService = new ClassService(new ClassRepository(context), context);
            _courseService = new CourseService(new CourseRepository(context));
            _teacherService = new TeacherService(new TeacherRepository(context));
            _scheduleService = new ScheduleService(new ScheduleRepository(context));
            _slotService = new SlotService(new SlotRepository(context));

            _currentClass = selectedClass;

            LoadData();
            BindData();
            LoadSchedules();
        }

        private void LoadData()
        {
            cbCourse.ItemsSource = _courseService.GetAllCourses();
            cbTeacher.ItemsSource = _teacherService.GetAllTeachers();
            cbSlot.ItemsSource = _slotService.GetAllSlots();

            cbRoom.ItemsSource = Enumerable.Range(1, 30)
                .Select(i => $"Room {i:D2}")
                .ToList();
        }

        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (cbDay.SelectedItem is ComboBoxItem dayItem)
            {
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

                byte day = byte.Parse(dayItem.Tag.ToString());

                var schedule = new Schedule
                {
                    DayOfWeek = day,
                    SlotId = (int)cbSlot.SelectedValue,
                    RoomName = cbRoom.SelectedItem?.ToString()
                };

                // ❌ check trùng
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

        private void LoadSchedules()
        {
            tempSchedules = _scheduleService
                .GetSchedulesByClassId(_currentClass.Id);

            dgSchedules.ItemsSource = tempSchedules;
        }

        private void BindData()
        {
            txtClassCode.Text = _currentClass.ClassCode;
            txtCapacity.Text = _currentClass.Capacity.ToString();

            cbCourse.SelectedValue = _currentClass.CourseId;
            cbTeacher.SelectedValue = _currentClass.TeacherId;

            dpStartDate.SelectedDate = _currentClass.StartDate;
            dpEndDate.SelectedDate = _currentClass.EndDate;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

                if (dpEndDate.SelectedDate < dpStartDate.SelectedDate)
                {
                    MessageBox.Show("EndDate phải lớn hơn StartDate!");
                    return;
                }

                // update object
                _currentClass.ClassCode = txtClassCode.Text;
                _currentClass.Capacity = capacity;
                _currentClass.CourseId = (int)cbCourse.SelectedValue;
                _currentClass.TeacherId = cbTeacher.SelectedValue != null
                                        ? (int)cbTeacher.SelectedValue
                                        : null;
                _currentClass.StartDate = dpStartDate.SelectedDate ?? DateTime.Now;
                _currentClass.EndDate = dpEndDate.SelectedDate ?? DateTime.Now;

                // 🔥 gọi đúng method
                _classService.UpdateClass(_currentClass, tempSchedules);

                MessageBox.Show("Cập nhật thành công!");
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
