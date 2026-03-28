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


        public OperationResult UpdateOwnProfile(Student student)
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();

                var existing = context.Students.FirstOrDefault(x => x.Id == student.Id);
                if (existing == null)
                    return OperationResult.Failure("Không tìm thấy sinh viên.");

                if (!existing.IsActive)
                    return OperationResult.Failure("Tài khoản sinh viên đang bị khóa.");

                var duplicatedEmail = context.Students.Any(x =>
                    x.Id != student.Id &&
                    x.Email.ToLower() == student.Email.ToLower());

                if (duplicatedEmail)
                    return OperationResult.Failure("Email đã được sử dụng bởi tài khoản khác.");

                // Chỉ cho sửa thông tin cá nhân
                existing.FullName = student.FullName;
                existing.Email = student.Email;
                existing.PhoneNumber = student.PhoneNumber;
                existing.DateOfBirth = student.DateOfBirth;
                existing.Gender = student.Gender;
                existing.Address = student.Address;

                context.SaveChanges();
                return OperationResult.Success("Cập nhật hồ sơ thành công.");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Lỗi khi cập nhật hồ sơ sinh viên: {ex.Message}");
            }
        }
    }
}