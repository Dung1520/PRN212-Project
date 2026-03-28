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

        public OperationResult RegisterStudent(Student student)
        {
            try
            {
                using var context = DbContextFactory.CreateDbContext();
                using var transaction = context.Database.BeginTransaction();

                var normalizedUsername = student.Username.Trim().ToLower();
                var normalizedEmail = student.Email.Trim().ToLower();

                var duplicatedUsername =
                    context.Students.Any(x => x.Username.ToLower() == normalizedUsername) ||
                    context.Teachers.Any(x => x.Username.ToLower() == normalizedUsername) ||
                    context.Admins.Any(x => x.Username.ToLower() == normalizedUsername);

                if (duplicatedUsername)
                    return OperationResult.Failure("Username đã tồn tại trong hệ thống.");

                var duplicatedEmail =
                    context.Students.Any(x => x.Email.ToLower() == normalizedEmail) ||
                    context.Teachers.Any(x => x.Email.ToLower() == normalizedEmail) ||
                    context.Admins.Any(x => x.Email.ToLower() == normalizedEmail);

                if (duplicatedEmail)
                    return OperationResult.Failure("Email đã tồn tại trong hệ thống.");

                student.StudentCode = GenerateNextStudentCode(context);
                student.IsActive = true;
                student.CreatedAt = DateTime.Now;

                context.Students.Add(student);
                context.SaveChanges();
                transaction.Commit();

                return OperationResult.Success(
                    $"Đăng ký tài khoản thành công. Mã học viên của bạn là {student.StudentCode}.");
            }
            catch (DbUpdateException ex)
            {
                return OperationResult.Failure(
                    $"Không thể lưu dữ liệu xuống database: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Lỗi khi đăng ký tài khoản: {ex.Message}");
            }
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

        private static string GenerateNextStudentCode(LctmsDbContext context)
        {
            var existingNumbers = context.Students
                .AsNoTracking()
                .Select(x => x.StudentCode)
                .Where(x => x.StartsWith("ST"))
                .AsEnumerable()
                .Select(code =>
                {
                    var numberPart = code.Length > 2 ? code[2..] : string.Empty;
                    return int.TryParse(numberPart, out var value) ? value : 0;
                });

            var nextNumber = existingNumbers.Any() ? existingNumbers.Max() + 1 : 1;
            return $"ST{nextNumber:000}";
        }
    }
}