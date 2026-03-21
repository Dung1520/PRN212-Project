using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StudentEnrollmentDto
    {
        public int EnrollmentId { get; set; }
        public int ClassId { get; set; }
        public string CourseName { get; set; } = null!;
        public string ClassCode { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string DayOfWeek { get; set; } = null!;
        public string Slot { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
