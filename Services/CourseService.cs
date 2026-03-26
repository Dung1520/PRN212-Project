using BusinessObjects;
using Repositories;
using System;
using System.Linq;

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
            ValidateCourse(course, isUpdate: false);
            course.CreatedAt = DateTime.Now;
            course.Status = string.IsNullOrWhiteSpace(course.Status) ? "Open" : course.Status.Trim();
            _repo.AddCourse(course);
        }

        public List<Course> GetAllCourses() => _repo.GetAllCourses();

        public Course? GetCourseById(int id) => _repo.GetCourseById(id);

        public void UpdateCourse(Course course)
        {
            var existing = _repo.GetCourseById(course.Id);
            if (existing == null)
                throw new Exception("Không tìm thấy khóa học.");

            ValidateCourse(course, isUpdate: true);
            course.CreatedAt = existing.CreatedAt;
            _repo.UpdateCourse(course);
        }

        public void DeleteCourse(int id)
        {
            throw new Exception("Theo yêu cầu hiện tại, module Course không dùng chức năng Delete.");
        }

        private void ValidateCourse(Course course, bool isUpdate)
        {
            if (course == null)
                throw new Exception("Dữ liệu khóa học không hợp lệ.");

            course.CourseCode = course.CourseCode?.Trim() ?? string.Empty;
            course.Name = course.Name?.Trim() ?? string.Empty;
            course.Description = course.Description?.Trim();
            course.SubjectCourse = course.SubjectCourse?.Trim();
            course.Status = string.IsNullOrWhiteSpace(course.Status) ? "Open" : course.Status.Trim();

            if (string.IsNullOrWhiteSpace(course.CourseCode))
                throw new Exception("Course code không được để trống.");

            if (string.IsNullOrWhiteSpace(course.Name))
                throw new Exception("Tên khóa học không được để trống.");

            if (course.DurationWeeks <= 0)
                throw new Exception("Số tuần phải lớn hơn 0.");

            if (course.Fee < 0)
                throw new Exception("Học phí phải lớn hơn hoặc bằng 0.");

            if (course.Status != "Open" && course.Status != "Closed")
                throw new Exception("Trạng thái khóa học chỉ được là Open hoặc Closed.");

            bool duplicateCode = _repo.GetAllCourses().Any(x =>
                x.CourseCode.Equals(course.CourseCode, StringComparison.OrdinalIgnoreCase)
                && (!isUpdate || x.Id != course.Id));

            if (duplicateCode)
                throw new Exception("Course code đã tồn tại.");
        }
    }
}
