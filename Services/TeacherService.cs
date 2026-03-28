using BusinessObjects;
using Repositories;
using System.Text.RegularExpressions;

namespace Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repo;

        public TeacherService()
        {
            _repo = new TeacherRepository();
        }

        public TeacherService(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
            => _repo.GetTeacherList(keyword);

        public List<Teacher> GetAllTeachers()
            => _repo.GetAllTeachers();

        public Teacher? GetTeacherById(int id)
            => _repo.GetTeacherById(id);

        public AdminTeacherDetailDto? GetTeacherDetailById(int id)
            => _repo.GetTeacherDetailById(id);

        public OperationResult UpdateOwnProfile(Teacher teacher)
        {
            if (teacher == null)
                return OperationResult.Failure("Dữ liệu không hợp lệ.");

            teacher.FullName = teacher.FullName?.Trim() ?? string.Empty;
            teacher.Email = teacher.Email?.Trim() ?? string.Empty;
            teacher.PhoneNumber = string.IsNullOrWhiteSpace(teacher.PhoneNumber) ? null : teacher.PhoneNumber.Trim();
            teacher.Gender = string.IsNullOrWhiteSpace(teacher.Gender) ? null : teacher.Gender.Trim();
            teacher.Address = string.IsNullOrWhiteSpace(teacher.Address) ? null : teacher.Address.Trim();

            if (string.IsNullOrWhiteSpace(teacher.FullName))
                return OperationResult.Failure("Họ và tên không được để trống.");

            if (teacher.FullName.Length > 100)
                return OperationResult.Failure("Họ và tên tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(teacher.Email))
                return OperationResult.Failure("Email không được để trống.");

            if (teacher.Email.Length > 100)
                return OperationResult.Failure("Email tối đa 100 ký tự.");

            if (!IsValidEmail(teacher.Email))
                return OperationResult.Failure("Email không đúng định dạng.");

            if (teacher.PhoneNumber != null)
            {
                if (teacher.PhoneNumber.Length > 20)
                    return OperationResult.Failure("Số điện thoại tối đa 20 ký tự.");

                if (!Regex.IsMatch(teacher.PhoneNumber, @"^[0-9+\-\s()]{8,20}$"))
                    return OperationResult.Failure("Số điện thoại không đúng định dạng.");
            }

            if (teacher.Gender != null &&
                teacher.Gender != "Male" &&
                teacher.Gender != "Female" &&
                teacher.Gender != "Other")
            {
                return OperationResult.Failure("Giới tính chỉ được là Male, Female hoặc Other.");
            }

            if (teacher.Address != null && teacher.Address.Length > 150)
                return OperationResult.Failure("Địa chỉ tối đa 150 ký tự.");

            if (teacher.DateOfBirth.HasValue)
            {
                if (teacher.DateOfBirth.Value.Date > DateTime.Today)
                    return OperationResult.Failure("Ngày sinh không được lớn hơn ngày hiện tại.");

                if (teacher.DateOfBirth.Value.Year < 1900)
                    return OperationResult.Failure("Ngày sinh không hợp lệ.");
            }

            return _repo.UpdateOwnProfile(teacher);
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
    }
}