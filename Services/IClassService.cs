using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IClassService
    {
        void AddClass(Class c, List<Schedule> schedules);
        List<Class> GetAllClasses();
        Class? GetClassById(int id);
        void UpdateClass(Class c, List<Schedule> schedules);
        void DeleteClass(int id);
    }
}
