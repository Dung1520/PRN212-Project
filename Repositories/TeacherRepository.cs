using BusinessObjects;
using DataAccess;

namespace Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeacherDao _dao = new TeacherDao();

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _dao.GetTeacherList(keyword);

        public Teacher? GetTeacherById(int id)
            => _dao.GetTeacherById(id);
    }
}