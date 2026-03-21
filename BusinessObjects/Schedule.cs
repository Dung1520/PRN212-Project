using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Schedule
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public byte DayOfWeek { get; set; } // 1 = Monday, 7 = Sunday
        public int SlotId { get; set; }
        public string? RoomName { get; set; }

    }
}