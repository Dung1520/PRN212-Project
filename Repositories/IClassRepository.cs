using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IClassRepository
    {
        void AddClass(Class c);
        List<Class> GetAllClasses();
        Class? GetClassById(int id);
        void UpdateClass(Class c);
        void DeleteClass(int id);
    }
}