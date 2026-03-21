using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CourseDAO _dao;

        public CourseRepository(LctmsDbContext context)
        {
            _dao = new CourseDAO(context);
        }

        public void AddCourse(Course course) => _dao.AddCourse(course);

        public List<Course> GetAllCourses() => _dao.GetAllCourses();

        public Course? GetCourseById(int id) => _dao.GetCourseById(id);

        public void UpdateCourse(Course course) => _dao.UpdateCourse(course);

        public void DeleteCourse(int id) => _dao.DeleteCourse(id);
    }
}
