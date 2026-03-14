using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}