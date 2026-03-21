using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class StudentDao
    {
        public List<StudentListItem> GetStudentList(string? keyword = null)
        {
            using var context = DbContextFactory.CreateDbContext();

            var query = context.Students
                .Select(s => new StudentListItem
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FullName = s.FullName,
                    Gender = s.Gender,
                    Email = s.Email,
                    PhoneNumber = s.PhoneNumber,
                    IsActive = s.IsActive
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                query = query.Where(s =>
                    s.StudentCode.ToLower().Contains(keyword) ||
                    s.FullName.ToLower().Contains(keyword) ||
                    s.Email.ToLower().Contains(keyword) ||
                    (s.PhoneNumber != null && s.PhoneNumber.ToLower().Contains(keyword)));
            }

            return query
                .OrderBy(s => s.FullName)
                .ToList();
        }

        public Student? GetStudentById(int id)
        {
            using var context = DbContextFactory.CreateDbContext();
            return context.Students
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == id);
        }

        public Student? GetByEmail(string email)
        {
            using var context = DbContextFactory.CreateDbContext();
            return context.Students
                .FirstOrDefault(x => x.Email == email && x.IsActive);
        }
    }
}