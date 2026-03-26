using System;
using System.Collections.Generic;

namespace BusinessObjects
{
    public class StudentScheduleDetailViewModel
    {
        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;

        public string SlotName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;

        public List<string> StudentNames { get; set; } = new();
    }
}
