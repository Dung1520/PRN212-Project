using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? SubjectCourse { get; set; }
        public int DurationWeeks { get; set; }
        public decimal Fee { get; set; }
        public string Status { get; set; } = "Open"; 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
