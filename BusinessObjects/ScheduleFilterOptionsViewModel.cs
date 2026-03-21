using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ScheduleFilterOptionsViewModel
    {
        public List<ScheduleFilterOptionItem> TeacherOptions { get; set; } = new();
        public List<ScheduleFilterOptionItem> CourseOptions { get; set; } = new();
        public List<ScheduleFilterOptionItem> ClassOptions { get; set; } = new();
        public List<ScheduleFilterOptionItem> SlotOptions { get; set; } = new();
    }
}
