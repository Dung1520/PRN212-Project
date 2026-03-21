namespace BusinessObjects
{
    public class EnrollmentApprovalItem
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;

        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;

        public int Capacity { get; set; }
        public int ApprovedCount { get; set; }
        public int RemainingSeats => Capacity - ApprovedCount;

        public DateTime StartDate { get; set; }
        public DateTime RegisteredAt { get; set; }

        public string EnrollmentStatus { get; set; } = string.Empty;
        public string ClassStatus { get; set; } = string.Empty;

        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }

        public bool ShowFinalStatus { get; set; }
        public string FinalStatusText { get; set; } = string.Empty;
    }
}