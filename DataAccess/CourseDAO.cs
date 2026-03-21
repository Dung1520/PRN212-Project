using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CourseDAO
    {
        private readonly LctmsDbContext _context;

        public CourseDAO(LctmsDbContext context)
        {
            _context = context;
        }

        public void AddCourse(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public List<Course> GetAllCourses()
        {
            return _context.Courses.ToList();
        }

        public Course? GetCourseById(int id)
        {
            return _context.Courses.FirstOrDefault(c => c.Id == id);
        }

        public void UpdateCourse(Course course)
        {
            var existing = _context.Courses.FirstOrDefault(c => c.Id == course.Id);
            if (existing != null)
            {
                existing.CourseCode = course.CourseCode;
                existing.Name = course.Name;
                existing.Description = course.Description;
                existing.SubjectCourse = course.SubjectCourse;
                existing.DurationWeeks = course.DurationWeeks;
                existing.Fee = course.Fee;
                existing.Status = course.Status;

                _context.SaveChanges();
            }
        }

        public void DeleteCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }
    }
}
