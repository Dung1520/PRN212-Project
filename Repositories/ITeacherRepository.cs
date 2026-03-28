using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BusinessObjects;

namespace Repositories
{
    public interface ITeacherRepository
    {
        List<TeacherListItem> GetTeacherList(string? keyword = null);
        List<Teacher> GetAllTeachers();
        Teacher? GetTeacherById(int id);
        Teacher? GetByEmail(string email);
        OperationResult UpdateOwnProfile(Teacher teacher);
    }
}
