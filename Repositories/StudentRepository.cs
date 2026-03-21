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
    }
}