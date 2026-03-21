using BusinessObjects;

namespace Repositories
{
    public interface IStudentCourseRepository
    {
        //student xem list course
        List<StudentCourseListDto> GetCourses(string? keyword, string? status);

        //student xem detail course
        StudentCourseDetailDto? GetCourseById(int courseId);

        //student xem class để đki
        List<StudentClassDto> GetClassesByCourseId(int courseId, int studentId);

        //xem ds đã enrollment
        List<StudentEnrollmentDto> GetStudentEnrollments(int studentId);
    }
}
