using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly ClassDAO _dao;

        public ClassRepository(LctmsDbContext context)
        {
            _dao = new ClassDAO(context);
        }

        public void AddClass(Class c) => _dao.AddClass(c);

        public List<Class> GetAllClasses() => _dao.GetAllClasses();

        public Class? GetClassById(int id) => _dao.GetClassById(id);

        public void UpdateClass(Class c) => _dao.UpdateClass(c);

        public void DeleteClass(int id) => _dao.DeleteClass(id);
    }
}
