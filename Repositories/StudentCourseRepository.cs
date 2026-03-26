using BusinessObjects;
using DataAccess;

namespace Repositories
{
    public class StudentCourseRepository : IStudentCourseRepository
    {
        public List<StudentCourseListDto> GetCourses(string? keyword, string? status)
        {
            using var context = new LctmsDbContext();

            var query = context.Courses.AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(c =>
                    c.CourseCode.Contains(keyword) ||
                    c.Name.Contains(keyword));
            }

            // filter
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

        //màn detail
        public StudentCourseDetailDto? GetCourseById(int courseId)
        {
            using var context = new LctmsDbContext();

            return context.Courses
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

        //xem class để đki
        public List<StudentClassDto> GetClassesByCourseId(int courseId, int studentId)
        {
            using var context = new LctmsDbContext();

            //  Tìm class đã đăng ký (nếu có)
            var enrolledClass = (
                from e in context.Enrollments
                join c in context.Classes on e.ClassId equals c.Id
                where e.StudentId == studentId
                      && (e.Status == "Pending" || e.Status == "Approved")
                      && c.CourseId == courseId
                select new { c.Id }
            ).FirstOrDefault();

            //  CASE 1: ĐÃ đăng ký → chỉ lấy class đó
            if (enrolledClass != null)
            {
                return (
                    from c in context.Classes
                    join s in context.Schedules on c.Id equals s.ClassId
                    join sl in context.Slots on s.SlotId equals sl.Id
                    join e in context.Enrollments on c.Id equals e.ClassId

                    where c.Id == enrolledClass.Id
                          && e.StudentId == studentId

                    select new StudentClassDto
                    {
                        Id = c.Id,
                        ClassCode = c.ClassCode,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Capacity = c.Capacity,
                        Status = c.Status,
                        EnrollmentStatus = e.Status,

                        CurrentEnrollment = context.Enrollments
                            .Count(x => x.ClassId == c.Id && x.Status == "Approved"),

                        DayOfWeek = s.DayOfWeek == 1 ? "Monday" :
                                    s.DayOfWeek == 2 ? "Tuesday" :
                                    s.DayOfWeek == 3 ? "Wednesday" :
                                    s.DayOfWeek == 4 ? "Thursday" :
                                    s.DayOfWeek == 5 ? "Friday" :
                                    s.DayOfWeek == 6 ? "Saturday" :
                                    "Sunday",

                        Slot = sl.SlotName + " (" + sl.StartTime + " - " + sl.EndTime + ")"
                    }
                )
                .AsEnumerable()
                .GroupBy(x => x.Id)
                .Select(g => g.First())
                .ToList();
            }

            //  CASE 2: CHƯA đăng ký → load class bình thường
            return (
                from c in context.Classes
                join s in context.Schedules on c.Id equals s.ClassId
                join sl in context.Slots on s.SlotId equals sl.Id

                where c.CourseId == courseId
                      && c.StartDate > DateTime.Now
                      && c.Status == "Open"

                select new StudentClassDto
                {
                    Id = c.Id,
                    ClassCode = c.ClassCode,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Capacity = c.Capacity,
                    Status = c.Status,
                    EnrollmentStatus = null,

                    CurrentEnrollment = context.Enrollments
                        .Count(x => x.ClassId == c.Id && x.Status == "Approved"),

                    DayOfWeek = s.DayOfWeek == 1 ? "Monday" :
                                s.DayOfWeek == 2 ? "Tuesday" :
                                s.DayOfWeek == 3 ? "Wednesday" :
                                s.DayOfWeek == 4 ? "Thursday" :
                                s.DayOfWeek == 5 ? "Friday" :
                                s.DayOfWeek == 6 ? "Saturday" :
                                "Sunday",

                    Slot = sl.SlotName + " (" + sl.StartTime + " - " + sl.EndTime + ")"
                }
            )
            .AsEnumerable()
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .Where(x => x.CurrentEnrollment < x.Capacity)
            .ToList();
        }

        //có thể hủy khi đki thành công
        public void CancelEnrollment(int studentId, int classId)
        {
            using var context = new LctmsDbContext();

            var enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId
                                  && e.ClassId == classId);

            if (enrollment == null)
                throw new Exception("Không tìm thấy đăng ký!");

            if (enrollment.Status != "Pending")
                throw new Exception("Chỉ được hủy khi đang Pending!");

            enrollment.Status = "Cancel";
            context.SaveChanges();
        }


        //xem ds đã đki
        public List<StudentEnrollmentDto> GetStudentEnrollments(int studentId)
        {
            using var context = new LctmsDbContext();

            var query =
                from e in context.Enrollments
                join c in context.Classes on e.ClassId equals c.Id
                join co in context.Courses on c.CourseId equals co.Id
                join s in context.Schedules on c.Id equals s.ClassId
                join sl in context.Slots on s.SlotId equals sl.Id

                where e.StudentId == studentId

                select new
                {
                    e.Id,
                    e.Status,
                    e.ClassId,
                    CourseName = co.Name,
                    c.ClassCode,
                    c.StartDate,
                    c.EndDate,

                    DayOfWeek = s.DayOfWeek == 1 ? "Monday" :
                                s.DayOfWeek == 2 ? "Tuesday" :
                                s.DayOfWeek == 3 ? "Wednesday" :
                                s.DayOfWeek == 4 ? "Thursday" :
                                s.DayOfWeek == 5 ? "Friday" :
                                s.DayOfWeek == 6 ? "Saturday" :
                                "Sunday",

                    Slot = sl.SlotName + " (" + sl.StartTime + " - " + sl.EndTime + ")"
                };

            return query
                .AsEnumerable()
                .GroupBy(x => new
                {
                    x.Id,
                    x.Status,
                    x.CourseName,
                    x.ClassCode,
                    x.StartDate,
                    x.EndDate,
                    x.Slot
                })
                .Select(g => new StudentEnrollmentDto
                {
                    EnrollmentId = g.Key.Id,
                    ClassId = g.First().ClassId,
                    Status = g.Key.Status,
                    CourseName = g.Key.CourseName,
                    ClassCode = g.Key.ClassCode,
                    StartDate = g.Key.StartDate,
                    EndDate = g.Key.EndDate,
                    Slot = g.Key.Slot,
                    DayOfWeek = string.Join(", ", g.Select(x => x.DayOfWeek).Distinct())
                })
                .ToList();
        }

    }

    

}