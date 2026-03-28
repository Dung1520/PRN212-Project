using BusinessObjects;

namespace Services
{
    public interface IStudentService
    {
        List<StudentListItem> GetStudentList(string? keyword = null);
        Student? GetStudentById(int id);
        AdminStudentDetailDto? GetStudentDetailById(int id);
        OperationResult UpdateOwnProfile(Student student);
        OperationResult RegisterStudent(StudentRegistrationRequest request);
    }
}