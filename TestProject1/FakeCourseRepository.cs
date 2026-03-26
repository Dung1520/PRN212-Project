using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    class FakeCourseRepository : ICourseRepository
    {
        public List<Course> courses = new List<Course>();

        public void AddCourse(Course course)
        {
            courses.Add(course);
        }

        public List<Course> GetAllCourses()
        {
            return courses;
        }

        public Course? GetCourseById(int id)
        {
            return courses.FirstOrDefault(c => c.Id == id);
        }

        public void UpdateCourse(Course course) { }

        public void DeleteCourse(int id) { }
    }
}
