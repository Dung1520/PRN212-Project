using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class StudentPreferenceDto
    {
        public string RawPrompt { get; set; } = string.Empty;
        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }

        public bool PreferMorning { get; set; }
        public bool PreferAfternoon { get; set; }
        public bool PreferEvening { get; set; }

        public string? LevelHint { get; set; } // Beginner, Elementary, Intermediate, Advanced
    }
}
