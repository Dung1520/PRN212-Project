using BusinessObjects;

namespace Services
{
    public interface ITeacherService
    {
        List<TeacherListItem> GetTeacherList(string? keyword = null);
        Teacher? GetTeacherById(int id);
    }
}