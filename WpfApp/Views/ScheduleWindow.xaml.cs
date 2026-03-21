using BusinessObjects;
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

namespace WpfApp.Views
{
    /// <summary>
    /// Interaction logic for ScheduleWindow.xaml
    /// </summary>
    public partial class ScheduleWindow : Window
    {
        private readonly IScheduleService _scheduleService;
        private readonly int _currentUserId;
        private readonly string _role;
        private DateTime _currentDate;

        private const int HeaderRowIndex = 0;
        private const int HeaderColumnIndex = 0;
        private const int TimeSlotColumnWidth = 130;
        private const int HeaderHeight = 60;
        private const int BodyRowHeight = 140;

        public ScheduleWindow(int currentUserId, string role)
        {
            InitializeComponent();
            _scheduleService = new ScheduleService();
            _currentUserId = currentUserId;
            _role = role;
            _currentDate = DateTime.Today;

            Loaded += ScheduleWindow_Loaded;
        }

        private void ScheduleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildGridStructure();
            LoadSchedule();
        }

        private void BuildGridStructure()
        {
            ScheduleGrid.RowDefinitions.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();
            ScheduleGrid.Children.Clear();

            ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(TimeSlotColumnWidth)
            });

            for (int i = 0; i < 7; i++)
            {
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            ScheduleGrid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(HeaderHeight)
            });

            for (int i = 0; i < 5; i++)
            {
                ScheduleGrid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(BodyRowHeight)
                });
            }
        }

        private void LoadSchedule()
        {
            var week = _scheduleService.GetWeeklySchedule(_currentUserId, _role, _currentDate);

            txtWeekRange.Text = $"Week: {week.WeekStartDate:dd MMM} - {week.WeekEndDate:dd MMM}, {week.WeekEndDate:yyyy}";

            RenderHeader(week.WeekStartDate);
            RenderBody(week);
        }

        private void RenderHeader(DateTime weekStart)
        {
            RemoveAllGridChildren();

            AddHeaderCell(HeaderRowIndex, HeaderColumnIndex, "Time Slot");

            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                string headerText = $"{date:dddd}\n{date:dd MMM}";
                AddHeaderCell(HeaderRowIndex, i + 1, headerText);
            }
        }

        private void RenderBody(ScheduleWeekViewModel week)
        {
            var slotGroups = week.Cells
                .GroupBy(x => new { x.SlotId, x.SlotName, x.StartTime, x.EndTime })
                .OrderBy(x => x.Key.StartTime)
                .ToList();

            int rowIndex = 1;

            foreach (var slotGroup in slotGroups)
            {
                string slotText = $"{slotGroup.Key.SlotName}\n{slotGroup.Key.StartTime:hh\\:mm}-{slotGroup.Key.EndTime:hh\\:mm}";
                AddSlotLabel(rowIndex, slotText);

                for (int day = 1; day <= 7; day++)
                {
                    var cellsOfDay = slotGroup
                        .Where(x => x.DayOfWeek == day)
                        .ToList();

                    AddScheduleCell(rowIndex, day, cellsOfDay);
                }

                rowIndex++;
            }
        }

        private void RemoveAllGridChildren()
        {
            ScheduleGrid.Children.Clear();
        }

        private void AddHeaderCell(int row, int column, string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(0.5),
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                Padding = new Thickness(6),
                Child = textBlock
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            ScheduleGrid.Children.Add(border);
        }

        private void AddSlotLabel(int row, string text)
        {
            var panel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var lines = text.Split('\n');
            if (lines.Length > 0)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = lines[0],
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
            }

            if (lines.Length > 1)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = lines[1],
                    FontSize = 11,
                    Foreground = Brushes.DimGray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                });
            }

            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White,
                Child = panel
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, 0);
            ScheduleGrid.Children.Add(border);
        }

        private void AddScheduleCell(int row, int dayOfWeek, List<ScheduleCellViewModel> cells)
        {
            var outerBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                BorderThickness = new Thickness(0.5),
                Background = Brushes.White
            };

            var container = new StackPanel
            {
                Margin = new Thickness(4)
            };

            foreach (var cell in cells.Where(x => x.HasClass))
            {
                var contentPanel = new StackPanel();

                contentPanel.Children.Add(new TextBlock
                {
                    Text = cell.ClassCode,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    Margin = new Thickness(0, 0, 0, 2)
                });

                contentPanel.Children.Add(new TextBlock
                {
                    Text = cell.RoomName,
                    FontSize = 11,
                    Foreground = Brushes.DimGray
                });

                var classBlock = new Border
                {
                    Margin = new Thickness(2),
                    Padding = new Thickness(6),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(145, 145, 145)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromRgb(246, 246, 246)),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Child = contentPanel,
                    Tag = cell
                };

                if (!string.Equals(_role, "Student", StringComparison.OrdinalIgnoreCase))
                {
                    classBlock.Cursor = Cursors.Hand;
                    classBlock.MouseLeftButtonUp += ScheduleCell_Click;
                }

                container.Children.Add(classBlock);
            }

            outerBorder.Child = container;

            Grid.SetRow(outerBorder, row);
            Grid.SetColumn(outerBorder, dayOfWeek);
            ScheduleGrid.Children.Add(outerBorder);
        }

        private void ScheduleCell_Click(object sender, MouseButtonEventArgs e)
        {
            if (string.Equals(_role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (sender is not FrameworkElement element || element.Tag is not ScheduleCellViewModel cell || !cell.ClassId.HasValue)
            {
                return;
            }

            if (string.Equals(_role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var detail = _scheduleService.GetAdminScheduleDetail(cell.ClassId.Value, cell.DayOfWeek, cell.SlotId);
                if (detail == null)
                {
                    MessageBox.Show("Schedule detail not found.");
                    return;
                }

                new AdminScheduleDetailWindow(detail).ShowDialog();
                return;
            }

            if (string.Equals(_role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                var detail = _scheduleService.GetTeacherScheduleDetail(_currentUserId, cell.ClassId.Value, cell.DayOfWeek, cell.SlotId);
                if (detail == null)
                {
                    MessageBox.Show("Schedule detail not found.");
                    return;
                }

                new TeacherScheduleDetailWindow(detail).ShowDialog();
            }
        }

        private void btnPrevWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(-7);
            LoadSchedule();
        }

        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Today;
            LoadSchedule();
        }

        private void btnNextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddDays(7);
            LoadSchedule();
        }
    }
}
