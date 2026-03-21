using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repository = new TeacherRepository();
        private readonly ITeacherRepository _repo;

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _repository.GetTeacherList(keyword);
        public TeacherService(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public List<Teacher> GetAllTeachers()
            => _repo.GetAllTeachers();

        public Teacher? GetTeacherById(int id)
            => _repository.GetTeacherById(id);
            => _repo.GetTeacherById(id);
    }
}
