using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories;


namespace Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;

        public CourseService(ICourseRepository repo)
        {
            _repo = repo;
        }

        public void AddCourse(Course course)
        {
            // 🔥 Validate
            if (string.IsNullOrWhiteSpace(course.CourseCode))
                throw new Exception("CourseCode is required");

            if (string.IsNullOrWhiteSpace(course.Name))
                throw new Exception("Course Name is required");

            if (course.DurationWeeks <= 0)
                throw new Exception("DurationWeeks must be > 0");

            if (course.Fee < 0)
                throw new Exception("Fee must be >= 0");

            // Auto set
            course.CreatedAt = DateTime.Now;
            course.Status = string.IsNullOrEmpty(course.Status) ? "Open" : course.Status;

            _repo.AddCourse(course);
        }

        public List<Course> GetAllCourses()
        {
            return _repo.GetAllCourses();
        }

        public Course? GetCourseById(int id)
        {
            return _repo.GetCourseById(id);
        }

        public void UpdateCourse(Course course)
        {
            var existing = _repo.GetCourseById(course.Id);
            if (existing == null)
                throw new Exception("Course not found");

            if (course.DurationWeeks <= 0)
                throw new Exception("DurationWeeks must be > 0");

            if (course.Fee < 0)
                throw new Exception("Fee must be >= 0");

            // ❗ Không update CreatedAt
            course.CreatedAt = existing.CreatedAt;

            _repo.UpdateCourse(course);
        }

        public void DeleteCourse(int id)
        {
            var existing = _repo.GetCourseById(id);
            if (existing == null)
                throw new Exception("Course not found");

            _repo.DeleteCourse(id);
        }
    }
}
