using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string? Role { get; set; } // Admin, Teacher, Student
        public object? User { get; set; }
        public string? Message { get; set; }
    }
}
