using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class TeacherDao
    {
        public List<TeacherListItem> GetTeacherList(string? keyword = null)
        {
            using var context = DbContextFactory.CreateDbContext();

            var query = context.Teachers
                .Select(t => new TeacherListItem
                {
                    Id = t.Id,
                    TeacherCode = t.TeacherCode,
                    FullName = t.FullName,
                    Gender = t.Gender,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    IsActive = t.IsActive
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                query = query.Where(t =>
                    t.TeacherCode.ToLower().Contains(keyword) ||
                    t.FullName.ToLower().Contains(keyword) ||
                    t.Email.ToLower().Contains(keyword) ||
                    (t.PhoneNumber != null && t.PhoneNumber.ToLower().Contains(keyword)));
            }

            return query
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public Teacher? GetTeacherById(int id)
        {
            using var context = DbContextFactory.CreateDbContext();
            return context.Teachers
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
        }
    }
}