using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class AiRecommendationCandidateDto
    {
        public int CandidateId { get; set; }   // dùng ClassId
        public int CourseId { get; set; }

        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Category { get; set; }

        public int DurationWeeks { get; set; }
        public decimal Fee { get; set; }

        public int ClassId { get; set; }
        public string ClassCode { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string DayOfWeek { get; set; } = string.Empty;
        public string Slot { get; set; } = string.Empty;

        public int Capacity { get; set; }
        public int CurrentEnrollment { get; set; }

        public int SeatsLeft => Capacity - CurrentEnrollment;
    }
}