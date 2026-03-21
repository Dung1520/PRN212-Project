using BusinessObjects;
using Repositories;

namespace Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repo;

        public TeacherService()
        {
            _repo = new TeacherRepository();
        }

        public TeacherService(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _repo.GetTeacherList(keyword);

        public List<Teacher> GetAllTeachers()
            => _repo.GetAllTeachers();

        public Teacher? GetTeacherById(int id)
            => _repo.GetTeacherById(id);
    }
}