using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StudentClassDto
    {
        public int Id { get; set; }
        public string ClassCode { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = null!;

        public string DayOfWeek { get; set; } = null!;
        public string Slot { get; set; } = null!;

        public int CurrentEnrollment { get; set; }

       // public string EnrollmentStatus { get; set; } = "";
        public string? EnrollmentStatus { get; set; }
    }
}
