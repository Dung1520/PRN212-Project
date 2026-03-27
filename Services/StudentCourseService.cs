using BusinessObjects;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly IStudentCourseRepository _repo;

        public StudentCourseService()
        {
            _repo = new StudentCourseRepository();
        }

        public List<StudentCourseListDto> GetCourses(string? keyword, string? status)
        {
            return _repo.GetCourses(keyword, status);
        }

        public StudentCourseDetailDto? GetCourseById(int courseId)
        {
            return _repo.GetCourseById(courseId);
        }

        public List<StudentClassDto> GetClassesByCourseId(int courseId, int studentId)
        {
            return _repo.GetClassesByCourseId(courseId, studentId);
        }

        public bool IsStudentAlreadyEnrolledInCourse(int studentId, int courseId)
        {
            using var context = new LctmsDbContext();

            return (from e in context.Enrollments
                    join c in context.Classes on e.ClassId equals c.Id
                    where e.StudentId == studentId
                          && (e.Status == "Pending" || e.Status == "Approved")
                          && c.CourseId == courseId
                    select e.Id).Any();
        }

        public void RegisterClass(int studentId, int classId)
        {
            using var context = new LctmsDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var student = context.Students.FirstOrDefault(x => x.Id == studentId);
                if (student == null || !student.IsActive)
                    throw new Exception("Học viên không tồn tại hoặc đã bị khóa.");

                var trainingClass = context.Classes.FirstOrDefault(x => x.Id == classId);
                if (trainingClass == null)
                    throw new Exception("Không tìm thấy lớp học.");

                var course = context.Courses.FirstOrDefault(x => x.Id == trainingClass.CourseId);
                if (course == null)
                    throw new Exception("Khóa học của lớp không tồn tại.");

                if (course.Status != "Open")
                    throw new Exception("Khóa học này hiện không cho phép đăng ký lớp.");

                if (trainingClass.Status != "Open")
                    throw new Exception("Chỉ được đăng ký lớp đang ở trạng thái Open.");

                if (trainingClass.StartDate.Date <= DateTime.Today)
                    throw new Exception("Lớp học đã bắt đầu hoặc sắp bắt đầu trong hôm nay, không thể đăng ký.");

                var activeSeatCount = context.Enrollments.Count(e =>
                    e.ClassId == classId &&
                    (e.Status == "Pending" || e.Status == "Approved"));

                if (activeSeatCount >= trainingClass.Capacity)
                    throw new Exception("Lớp đã đủ chỗ theo số lượng đăng ký hiện tại (Pending/Approved).");

                var sameClassEnrollment = context.Enrollments
                    .FirstOrDefault(e => e.StudentId == studentId && e.ClassId == classId);

                if (sameClassEnrollment != null)
                {
                    if (sameClassEnrollment.Status == "Pending" || sameClassEnrollment.Status == "Approved")
                        throw new Exception("Bạn đã có đăng ký Pending/Approved cho lớp này rồi.");

                    if (sameClassEnrollment.Status == "Cancel")
                    {
                        EnsureStudentCanRegister(context, studentId, trainingClass, classId);
                        sameClassEnrollment.Status = "Pending";
                        sameClassEnrollment.RegisteredAt = DateTime.Now;
                        context.SaveChanges();
                        transaction.Commit();
                        return;
                    }

                    if (sameClassEnrollment.Status == "Rejected")
                    {
                        EnsureStudentCanRegister(context, studentId, trainingClass, classId);
                        sameClassEnrollment.Status = "Pending";
                        sameClassEnrollment.RegisteredAt = DateTime.Now;
                        context.SaveChanges();
                        transaction.Commit();
                        return;
                    }
                }

                EnsureStudentCanRegister(context, studentId, trainingClass, classId);

                var newEnrollment = new Enrollment
                {
                    StudentId = studentId,
                    ClassId = classId,
                    Status = "Pending",
                    RegisteredAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                context.Enrollments.Add(newEnrollment);
                context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void CancelEnrollment(int studentId, int classId)
        {
            using var context = new LctmsDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var enrollment = context.Enrollments
                    .FirstOrDefault(e => e.StudentId == studentId && e.ClassId == classId);

                if (enrollment == null)
                    throw new Exception("Không tìm thấy đăng ký!");

                if (enrollment.Status != "Pending")
                    throw new Exception("Chỉ được hủy khi đang Pending!");

                enrollment.Status = "Cancel";
                context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<StudentEnrollmentDto> GetStudentEnrollments(int studentId)
        {
            return _repo.GetStudentEnrollments(studentId);
        }

        private static void EnsureStudentCanRegister(LctmsDbContext context, int studentId, Class targetClass, int targetClassId)
        {
            var hasSameCourse =
                (from e in context.Enrollments
                 join c in context.Classes on e.ClassId equals c.Id
                 where e.StudentId == studentId
                       && e.ClassId != targetClassId
                       && (e.Status == "Pending" || e.Status == "Approved")
                       && c.CourseId == targetClass.CourseId
                 select e.Id).Any();

            if (hasSameCourse)
                throw new Exception("Bạn đã có lớp Pending/Approved khác của cùng khóa học.");

            var targetSchedules = GetSchedulePairs(context, targetClassId);
            if (targetSchedules.Count == 0)
                throw new Exception("Lớp học chưa có lịch học hợp lệ.");

            var conflictingClass =
                (from e in context.Enrollments
                 join c in context.Classes on e.ClassId equals c.Id
                 where e.StudentId == studentId
                       && e.ClassId != targetClassId
                       && (e.Status == "Pending" || e.Status == "Approved")
                       && c.StartDate <= targetClass.EndDate
                       && c.EndDate >= targetClass.StartDate
                 select new { c.Id, c.ClassCode })
                 .AsEnumerable()
                 .FirstOrDefault(x => HasScheduleConflict(context, x.Id, targetSchedules));

            if (conflictingClass != null)
                throw new Exception($"Lớp bị trùng lịch với lớp {conflictingClass.ClassCode} mà bạn đã đăng ký Pending/Approved.");
        }

        private static HashSet<(int DayOfWeek, int SlotId)> GetSchedulePairs(LctmsDbContext context, int classId)
        {
            return context.Schedules
                .Where(x => x.ClassId == classId)
                .Select(x => new { x.DayOfWeek, x.SlotId })
                .AsEnumerable()
                .Select(x => ((int)x.DayOfWeek, x.SlotId))
                .ToHashSet();
        }

        private static bool HasScheduleConflict(
            LctmsDbContext context,
            int existingClassId,
            HashSet<(int DayOfWeek, int SlotId)> targetSchedules)
        {
            var existingSchedules = context.Schedules
                .Where(x => x.ClassId == existingClassId)
                .Select(x => new { x.DayOfWeek, x.SlotId })
                .AsEnumerable()
               .Select(x => ((int)x.DayOfWeek, x.SlotId));

            return existingSchedules.Any(x => targetSchedules.Contains(x));
        }
    }
}