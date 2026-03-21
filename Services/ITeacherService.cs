using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ITeacherService
    {
        List<TeacherListItem> GetTeacherList(string? keyword = null);
        List<Teacher> GetAllTeachers();
        Teacher? GetTeacherById(int id);
    }
}
