using BusinessObjects;
using DataAccess;
using Repositories;
using System.Text.RegularExpressions;

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

            using var context = new LctmsDbContext();
            bool hasClasses = context.Classes.Any(x => x.CourseId == course.Id);

            if (hasClasses)
            {
                if (!string.Equals(existing.CourseCode, course.CourseCode, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Khóa học đã có lớp, không được sửa CourseCode.");

                if (existing.DurationWeeks != course.DurationWeeks)
                    throw new Exception("Khóa học đã có lớp, không được sửa DurationWeeks.");

                if (existing.Fee != course.Fee)
                    throw new Exception("Khóa học đã có lớp, không được sửa Fee.");
            }

            bool hasOpenClasses = context.Classes.Any(x => x.CourseId == course.Id && x.Status == "Open");
            if (course.Status == "Closed" && hasOpenClasses)
                throw new Exception("Không thể đóng course khi vẫn còn class đang Open.");

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

            if (!Regex.IsMatch(course.CourseCode, @"^[A-Za-z0-9\-]+$"))
                throw new Exception("Course code chỉ nên gồm chữ, số và dấu gạch ngang.");

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

            bool duplicateName = _repo.GetAllCourses().Any(x =>
                x.Name.Equals(course.Name, StringComparison.OrdinalIgnoreCase)
                && (!isUpdate || x.Id != course.Id));

            if (duplicateName)
                throw new Exception("Tên khóa học đã tồn tại.");
        }
    }
}
