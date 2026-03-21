using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public static class Session
    {
        public static string Role { get; set; } = "";
        public static object? User { get; set; }
    }
}
