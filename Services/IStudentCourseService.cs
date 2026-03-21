using BusinessObjects;

namespace Services
{
    public interface IStudentCourseService
    {
        List<StudentCourseListDto> GetCourses(string? keyword, string? status);
        StudentCourseDetailDto? GetCourseById(int courseId);

        List<StudentClassDto> GetClassesByCourseId(int courseId,int studentId);

        bool IsStudentAlreadyEnrolledInCourse(int studentId, int courseId);
        void RegisterClass(int studentId, int classId);
        void CancelEnrollment(int studentId, int classId);
       

        //xem ds đã đăng kí
        List<StudentEnrollmentDto> GetStudentEnrollments(int studentId);
    }
}