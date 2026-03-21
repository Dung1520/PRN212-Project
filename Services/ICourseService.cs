using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;

namespace Services
{
    public interface ICourseService
    {
        void AddCourse(Course course);
        List<Course> GetAllCourses();
        Course? GetCourseById(int id);
        void UpdateCourse(Course course);
        void DeleteCourse(int id);
    }
}
