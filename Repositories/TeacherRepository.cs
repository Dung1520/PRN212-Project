using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly TeacherDao _dao = new TeacherDao();
        private readonly TeacherDAO _dao;

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _dao.GetTeacherList(keyword);
        public TeacherRepository(LctmsDbContext context)
        {
            _dao = new TeacherDAO(context);
        }

        public List<Teacher> GetAllTeachers()
            => _dao.GetAllTeachers();

        public Teacher? GetTeacherById(int id)
            => _dao.GetTeacherById(id);
    }
}
