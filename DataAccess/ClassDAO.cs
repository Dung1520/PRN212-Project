using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class ClassDAO
    {
        private readonly LctmsDbContext _context;

        public ClassDAO(LctmsDbContext context)
        {
            _context = context;
        }

        public void AddClass(Class c)
        {
            _context.Classes.Add(c);
            _context.SaveChanges();
        }

        public List<Class> GetAllClasses()
        {
            return _context.Classes
                .AsNoTracking()
                .OrderBy(x => x.ClassCode)
                .ToList();
        }

        public Class? GetClassById(int id)
        {
            return _context.Classes
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

        public void UpdateClass(Class c)
        {
            var existing = _context.Classes.FirstOrDefault(x => x.Id == c.Id);
            if (existing != null)
            {
                existing.CourseId = c.CourseId;
                existing.TeacherId = c.TeacherId;
                existing.ClassCode = c.ClassCode;
                existing.StartDate = c.StartDate;
                existing.EndDate = c.EndDate;
                existing.Capacity = c.Capacity;
                existing.Status = c.Status;

                _context.SaveChanges();
            }
        }

        public void DeleteClass(int id)
        {
            var c = _context.Classes.FirstOrDefault(x => x.Id == id);
            if (c != null)
            {
                _context.Classes.Remove(c);
                _context.SaveChanges();
            }
        }
    }
}