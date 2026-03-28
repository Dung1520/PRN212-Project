using BusinessObjects;

namespace Repositories
{
    public interface IStudentRepository
    {
        List<StudentListItem> GetStudentList(string? keyword = null);
        Student? GetStudentById(int id);
        Student? GetByEmail(string email);
        OperationResult UpdateOwnProfile(Student student);
        OperationResult RegisterStudent(Student student);
    }
}