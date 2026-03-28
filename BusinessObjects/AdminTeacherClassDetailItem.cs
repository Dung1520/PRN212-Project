namespace BusinessObjects
{
    public class AdminTeacherClassDetailItem
    {
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public string ClassStatus { get; set; } = string.Empty;
        public int ApprovedEnrollmentCount { get; set; }
        public int PendingEnrollmentCount { get; set; }
        public string ScheduleText { get; set; } = string.Empty;
    }
}