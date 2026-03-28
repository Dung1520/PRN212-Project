using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class TeacherDAO
    {
        private readonly LctmsDbContext? _context;

        public TeacherDAO()
        {
        }

        public TeacherDAO(LctmsDbContext context)
        {
            _context = context;
        }

        private LctmsDbContext GetContext()
        {
            return _context ?? DbContextFactory.CreateDbContext();
        }

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            var query = db.Teachers
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

        public AdminTeacherDetailDto? GetTeacherDetailById(int id)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            var teacher = db.Teachers
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);

            if (teacher == null)
                return null;

            var classRows =
                (from c in db.Classes.AsNoTracking()
                 join co in db.Courses.AsNoTracking() on c.CourseId equals co.Id
                 where c.TeacherId == id
                 orderby c.StartDate descending, c.ClassCode
                 select new
                 {
                     Class = c,
                     Course = co,
                     ApprovedCount = db.Enrollments.Count(e => e.ClassId == c.Id && e.Status == "Approved"),
                     PendingCount = db.Enrollments.Count(e => e.ClassId == c.Id && e.Status == "Pending")
                 }).ToList();

            var classIds = classRows.Select(x => x.Class.Id).Distinct().ToList();
            var scheduleRows = db.Schedules
                .AsNoTracking()
                .Where(s => classIds.Contains(s.ClassId))
                .Join(db.Slots.AsNoTracking(),
                    s => s.SlotId,
                    sl => sl.Id,
                    (s, sl) => new
                    {
                        s.ClassId,
                        s.DayOfWeek,
                        SlotName = sl.SlotName,
                        sl.StartTime,
                        sl.EndTime,
                        s.RoomName
                    })
                .ToList()
                .GroupBy(x => x.ClassId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(" | ", g.OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartTime)
                        .Select(x => $"{GetDayName(x.DayOfWeek)} - {x.SlotName} ({x.StartTime:hh\\:mm}-{x.EndTime:hh\\:mm}){(string.IsNullOrWhiteSpace(x.RoomName) ? string.Empty : $" - {x.RoomName}")}")));

            return new AdminTeacherDetailDto
            {
                Id = teacher.Id,
                TeacherCode = teacher.TeacherCode,
                Username = teacher.Username,
                FullName = teacher.FullName,
                Email = teacher.Email,
                PhoneNumber = teacher.PhoneNumber,
                DateOfBirth = teacher.DateOfBirth,
                Gender = teacher.Gender,
                Address = teacher.Address,
                IsActive = teacher.IsActive,
                CreatedAt = teacher.CreatedAt,
                TotalTeachingClasses = classRows.Count,
                OpenClassCount = classRows.Count(x => x.Class.Status == "Open"),
                FullClassCount = classRows.Count(x => x.Class.Status == "Full"),
                ClosedClassCount = classRows.Count(x => x.Class.Status == "Closed"),
                TeachingClasses = classRows.Select(x => new AdminTeacherClassDetailItem
                {
                    ClassId = x.Class.Id,
                    CourseId = x.Course.Id,
                    CourseCode = x.Course.CourseCode,
                    CourseName = x.Course.Name,
                    ClassCode = x.Class.ClassCode,
                    StartDate = x.Class.StartDate,
                    EndDate = x.Class.EndDate,
                    Capacity = x.Class.Capacity,
                    ClassStatus = x.Class.Status,
                    ApprovedEnrollmentCount = x.ApprovedCount,
                    PendingEnrollmentCount = x.PendingCount,
                    ScheduleText = scheduleRows.TryGetValue(x.Class.Id, out var text) ? text : string.Empty
                }).ToList()
            };
        }

        public List<Teacher> GetAllTeachers()
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .AsNoTracking()
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public Teacher? GetTeacherById(int id)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
        }

        public Teacher? GetByEmail(string email)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .FirstOrDefault(x => x.Email == email && x.IsActive);
        }

        public OperationResult UpdateOwnProfile(Teacher teacher)
        {
            try
            {
                using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
                var db = _context ?? context!;

                var existing = db.Teachers.FirstOrDefault(x => x.Id == teacher.Id);
                if (existing == null)
                    return OperationResult.Failure("Không tìm thấy giáo viên.");

                if (!existing.IsActive)
                    return OperationResult.Failure("Tài khoản giáo viên đang bị khóa.");

                var duplicatedEmail = db.Teachers.Any(x =>
                    x.Id != teacher.Id &&
                    x.Email.ToLower() == teacher.Email.ToLower());

                if (duplicatedEmail)
                    return OperationResult.Failure("Email đã được sử dụng bởi tài khoản khác.");

                existing.FullName = teacher.FullName;
                existing.Email = teacher.Email;
                existing.PhoneNumber = teacher.PhoneNumber;
                existing.DateOfBirth = teacher.DateOfBirth;
                existing.Gender = teacher.Gender;
                existing.Address = teacher.Address;

                db.SaveChanges();
                return OperationResult.Success("Cập nhật hồ sơ thành công.");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Lỗi khi cập nhật hồ sơ giáo viên: {ex.Message}");
            }
        }

        private static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Mon",
                2 => "Tue",
                3 => "Wed",
                4 => "Thu",
                5 => "Fri",
                6 => "Sat",
                7 => "Sun",
                _ => "N/A"
            };
        }
    }
}