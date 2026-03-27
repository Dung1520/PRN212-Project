using BusinessObjects;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class StudentCourseRepository : IStudentCourseRepository
    {
        public List<StudentCourseListDto> GetCourses(string? keyword, string? status)
        {
            using var context = new LctmsDbContext();

            var query = context.Courses
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(c =>
                    c.CourseCode.Contains(keyword) ||
                    c.Name.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(c => c.Status == status);
            }

            return query
                .OrderBy(c => c.CourseCode)
                .Select(c => new StudentCourseListDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.Name,
                    Category = c.SubjectCourse,
                    DurationWeeks = c.DurationWeeks,
                    Fee = c.Fee,
                    Status = c.Status
                })
                .ToList();
        }

        public StudentCourseDetailDto? GetCourseById(int courseId)
        {
            using var context = new LctmsDbContext();

            return context.Courses
                .AsNoTracking()
                .Where(c => c.Id == courseId)
                .Select(c => new StudentCourseDetailDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.Name,
                    Category = c.SubjectCourse,
                    DurationWeeks = c.DurationWeeks,
                    Fee = c.Fee,
                    Status = c.Status,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefault();
        }

        public List<StudentClassDto> GetClassesByCourseId(int courseId, int studentId)
        {
            using var context = new LctmsDbContext();

            var activeEnrollment = (
                from e in context.Enrollments.AsNoTracking()
                join c in context.Classes.AsNoTracking() on e.ClassId equals c.Id
                where e.StudentId == studentId
                      && (e.Status == "Pending" || e.Status == "Approved")
                      && c.CourseId == courseId
                select new
                {
                    e.ClassId,
                    e.Status
                }
            ).FirstOrDefault();

            if (activeEnrollment != null)
            {
                return BuildClassDtos(
                    context,
                    new List<int> { activeEnrollment.ClassId },
                    activeEnrollment.ClassId,
                    activeEnrollment.Status,
                    onlyAvailable: false);
            }

            var availableClassIds = context.Classes
                .AsNoTracking()
                .Where(c => c.CourseId == courseId
                            && c.StartDate > DateTime.Today
                            && c.Status == "Open")
                .Select(c => c.Id)
                .ToList();

            return BuildClassDtos(
                context,
                availableClassIds,
                null,
                null,
                onlyAvailable: true);
        }

        public List<StudentEnrollmentDto> GetStudentEnrollments(int studentId)
        {
            using var context = new LctmsDbContext();

            var enrollmentRows = (
                from e in context.Enrollments.AsNoTracking()
                join c in context.Classes.AsNoTracking() on e.ClassId equals c.Id
                join co in context.Courses.AsNoTracking() on c.CourseId equals co.Id
                where e.StudentId == studentId
                select new
                {
                    EnrollmentId = e.Id,
                    e.Status,
                    ClassId = c.Id,
                    CourseName = co.Name,
                    c.ClassCode,
                    c.StartDate,
                    c.EndDate
                }
            ).ToList();

            if (!enrollmentRows.Any())
            {
                return new List<StudentEnrollmentDto>();
            }

            var classIds = enrollmentRows
                .Select(x => x.ClassId)
                .Distinct()
                .ToList();

            var schedules = context.Schedules
                .AsNoTracking()
                .Where(s => classIds.Contains(s.ClassId))
                .Select(s => new
                {
                    s.ClassId,
                    DayOfWeek = (int)s.DayOfWeek,
                    s.SlotId
                })
                .ToList();

            var slotIds = schedules
                .Select(x => x.SlotId)
                .Distinct()
                .ToList();

            var slots = context.Slots
                .AsNoTracking()
                .Where(sl => slotIds.Contains(sl.Id))
                .Select(sl => new
                {
                    sl.Id,
                    sl.SlotName,
                    sl.StartTime,
                    sl.EndTime
                })
                .ToList();

            var result = new List<StudentEnrollmentDto>();

            foreach (var item in enrollmentRows)
            {
                var classSchedules = schedules
                    .Where(x => x.ClassId == item.ClassId)
                    .ToList();

                var dayText = string.Join(", ",
                    classSchedules
                        .Select(x => ConvertDayOfWeek(x.DayOfWeek))
                        .Distinct());

                var slotText = string.Join(" | ",
                    classSchedules
                        .Join(
                            slots,
                            s => s.SlotId,
                            sl => sl.Id,
                            (s, sl) => $"{sl.SlotName} ({sl.StartTime:hh\\:mm} - {sl.EndTime:hh\\:mm})"
                        )
                        .Distinct());

                result.Add(new StudentEnrollmentDto
                {
                    EnrollmentId = item.EnrollmentId,
                    ClassId = item.ClassId,
                    CourseName = item.CourseName,
                    ClassCode = item.ClassCode,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    DayOfWeek = dayText,
                    Slot = slotText,
                    Status = item.Status
                });
            }

            return result
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.ClassCode)
                .ToList();
        }

        private List<StudentClassDto> BuildClassDtos(
            LctmsDbContext context,
            List<int> classIds,
            int? currentClassId,
            string? currentEnrollmentStatus,
            bool onlyAvailable)
        {
            if (classIds == null || classIds.Count == 0)
            {
                return new List<StudentClassDto>();
            }

            var classes = context.Classes
                .AsNoTracking()
                .Where(c => classIds.Contains(c.Id))
                .Select(c => new
                {
                    c.Id,
                    c.ClassCode,
                    c.StartDate,
                    c.EndDate,
                    c.Capacity,
                    c.Status
                })
                .ToList();

            var schedules = context.Schedules
                .AsNoTracking()
                .Where(s => classIds.Contains(s.ClassId))
                .Select(s => new
                {
                    s.ClassId,
                    DayOfWeek = (int)s.DayOfWeek,
                    s.SlotId
                })
                .ToList();

            var slotIds = schedules
                .Select(x => x.SlotId)
                .Distinct()
                .ToList();

            var slots = context.Slots
                .AsNoTracking()
                .Where(sl => slotIds.Contains(sl.Id))
                .Select(sl => new
                {
                    sl.Id,
                    sl.SlotName,
                    sl.StartTime,
                    sl.EndTime
                })
                .ToList();

            var approvedCounts = context.Enrollments
                .AsNoTracking()
                .Where(e => classIds.Contains(e.ClassId) && e.Status == "Approved")
                .GroupBy(e => e.ClassId)
                .Select(g => new
                {
                    ClassId = g.Key,
                    Count = g.Count()
                })
                .ToList()
                .ToDictionary(x => x.ClassId, x => x.Count);

            var result = new List<StudentClassDto>();

            foreach (var c in classes)
            {
                var classSchedules = schedules
                    .Where(x => x.ClassId == c.Id)
                    .ToList();

                var dayText = string.Join(", ",
                    classSchedules
                        .Select(x => ConvertDayOfWeek(x.DayOfWeek))
                        .Distinct());

                var slotText = string.Join(" | ",
                    classSchedules
                        .Join(
                            slots,
                            s => s.SlotId,
                            sl => sl.Id,
                            (s, sl) => $"{sl.SlotName} ({sl.StartTime:hh\\:mm} - {sl.EndTime:hh\\:mm})"
                        )
                        .Distinct());

                var currentEnrollment = approvedCounts.TryGetValue(c.Id, out var count)
                    ? count
                    : 0;

                if (onlyAvailable && currentEnrollment >= c.Capacity)
                {
                    continue;
                }

                result.Add(new StudentClassDto
                {
                    Id = c.Id,
                    ClassCode = c.ClassCode,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    CurrentEnrollment = currentEnrollment,
                    EnrollmentStatus = c.Id == currentClassId ? currentEnrollmentStatus : null,
                    DayOfWeek = dayText,
                    Slot = slotText
                });
            }

            return result
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.ClassCode)
                .ToList();
        }

        private static string ConvertDayOfWeek(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                7 => "Sunday",
                _ => $"Day {dayOfWeek}"
            };
        }
    }
}