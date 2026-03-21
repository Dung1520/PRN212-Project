using BusinessObjects;
using Repositories;

namespace Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository = new TeacherRepository();

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _repository.GetTeacherList(keyword);

        public Teacher? GetTeacherById(int id)
            => _repository.GetTeacherById(id);
    }
}