using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ScheduleCellViewModel
    {
        public int DayOfWeek { get; set; }
        public int SlotId { get; set; }
        public string SlotName { get; set; } = string.Empty;

        public int? ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool HasClass => ClassId.HasValue;

        public string DisplayText =>
            HasClass
                ? $"{ClassCode}\n{RoomName}"
                : string.Empty;
    }
}
