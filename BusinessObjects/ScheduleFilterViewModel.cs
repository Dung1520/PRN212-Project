using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class ScheduleFilterViewModel
    {
        public string? Keyword { get; set; }
        public int? TeacherId { get; set; }
        public int? CourseId { get; set; }
        public int? ClassId { get; set; }
        public int? SlotId { get; set; }
    }
}
