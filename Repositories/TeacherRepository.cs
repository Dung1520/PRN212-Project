using BusinessObjects;
using DataAccess;

namespace Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeacherDAO _dao;

        public TeacherRepository()
        {
            _dao = new TeacherDAO();
        }

        public TeacherRepository(LctmsDbContext context)
        {
            _dao = new TeacherDAO(context);
        }

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _dao.GetTeacherList(keyword);

        public List<Teacher> GetAllTeachers()
            => _dao.GetAllTeachers();

        public Teacher? GetTeacherById(int id)
            => _dao.GetTeacherById(id);
    }
}