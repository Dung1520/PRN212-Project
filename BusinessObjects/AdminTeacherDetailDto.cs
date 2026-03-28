namespace BusinessObjects
{
    public class AdminTeacherDetailDto
    {
        public int Id { get; set; }
        public string TeacherCode { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalTeachingClasses { get; set; }
        public int OpenClassCount { get; set; }
        public int FullClassCount { get; set; }
        public int ClosedClassCount { get; set; }
        public List<AdminTeacherClassDetailItem> TeachingClasses { get; set; } = new();
    }
}