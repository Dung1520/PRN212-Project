using BusinessObjects;
using DataAccess;
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

            var enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId && e.ClassId == classId);

            if (enrollment != null)
            {
                if (enrollment.Status == "Cancel")
                {
                    enrollment.Status = "Pending";
                    context.SaveChanges();
                    return;
                }

                if (enrollment.Status == "Pending" || enrollment.Status == "Approved")
                    throw new Exception("Bạn đã đăng ký lớp này rồi!");
            }

            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                ClassId = classId,
                Status = "Pending"
            };

            context.Enrollments.Add(newEnrollment);
            context.SaveChanges();
        }

        public void CancelEnrollment(int studentId, int classId)
        {
            using var context = new LctmsDbContext();

            var enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId && e.ClassId == classId);

            if (enrollment == null)
                throw new Exception("Không tìm thấy đăng ký!");

            if (enrollment.Status != "Pending")
                throw new Exception("Chỉ được hủy khi đang Pending!");

            enrollment.Status = "Cancel";
            context.SaveChanges();
        }

        public List<StudentEnrollmentDto> GetStudentEnrollments(int studentId)
        {
            return _repo.GetStudentEnrollments(studentId);
        }
    }
}