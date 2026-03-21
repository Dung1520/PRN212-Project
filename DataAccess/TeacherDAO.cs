using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TeacherDAO
    {
        private readonly LctmsDbContext _context;

        public TeacherDAO(LctmsDbContext context)
        {
            _context = context;
        }

        public List<Teacher> GetAllTeachers()
        {
            return _context.Teachers.ToList();
        }

        public Teacher? GetTeacherById(int id)
        {
            return _context.Teachers.FirstOrDefault(t => t.Id == id);
        }
    }
}
