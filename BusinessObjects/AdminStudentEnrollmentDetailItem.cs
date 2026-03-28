namespace BusinessObjects
{
    public class AdminStudentEnrollmentDetailItem
    {
        public int EnrollmentId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string EnrollmentStatus { get; set; } = string.Empty;
        public string ClassStatus { get; set; } = string.Empty;
        public string? TeacherName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleText { get; set; } = string.Empty;
    }
}