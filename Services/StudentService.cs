using BusinessObjects;
using Repositories;

namespace Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository = new StudentRepository();

        public List<StudentListItem> GetStudentList(string? keyword = null)
            => _repository.GetStudentList(keyword);

        public Student? GetStudentById(int id)
            => _repository.GetStudentById(id);
    }
}