using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StudentCourseDetailDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string? Category { get; set; }
        public int DurationWeeks { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
