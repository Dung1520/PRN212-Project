using BusinessObjects;
using DataAccess;

namespace Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly StudentDao _dao = new StudentDao();

        public List<StudentListItem> GetStudentList(string? keyword = null)
            => _dao.GetStudentList(keyword);

        public Student? GetStudentById(int id)
            => _dao.GetStudentById(id);

        public AdminStudentDetailDto? GetStudentDetailById(int id)
            => _dao.GetStudentDetailById(id);

        public Student? GetByEmail(string email)
            => _dao.GetByEmail(email);

        public OperationResult UpdateOwnProfile(Student student)
            => _dao.UpdateOwnProfile(student);

        public OperationResult RegisterStudent(Student student)
            => _dao.RegisterStudent(student);
    }
}