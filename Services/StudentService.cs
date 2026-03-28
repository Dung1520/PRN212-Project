using BusinessObjects;
using Repositories;
using System.Text.RegularExpressions;

namespace Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _repository = new StudentRepository();

        public List<StudentListItem> GetStudentList(string? keyword = null)
            => _repository.GetStudentList(keyword);

        public Student? GetStudentById(int id)
            => _repository.GetStudentById(id);
        public OperationResult UpdateOwnProfile(Student student)
        {
            if (student == null)
                return OperationResult.Failure("Dữ liệu không hợp lệ.");

            student.FullName = student.FullName?.Trim() ?? string.Empty;
            student.Email = student.Email?.Trim() ?? string.Empty;
            student.PhoneNumber = string.IsNullOrWhiteSpace(student.PhoneNumber) ? null : student.PhoneNumber.Trim();
            student.Gender = string.IsNullOrWhiteSpace(student.Gender) ? null : student.Gender.Trim();
            student.Address = string.IsNullOrWhiteSpace(student.Address) ? null : student.Address.Trim();

            if (string.IsNullOrWhiteSpace(student.FullName))
                return OperationResult.Failure("Họ và tên không được để trống.");

            if (student.FullName.Length > 100)
                return OperationResult.Failure("Họ và tên tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(student.Email))
                return OperationResult.Failure("Email không được để trống.");

            if (student.Email.Length > 100)
                return OperationResult.Failure("Email tối đa 100 ký tự.");

            if (!IsValidEmail(student.Email))
                return OperationResult.Failure("Email không đúng định dạng.");

            if (student.PhoneNumber != null)
            {
                if (student.PhoneNumber.Length > 20)
                    return OperationResult.Failure("Số điện thoại tối đa 20 ký tự.");

                if (!Regex.IsMatch(student.PhoneNumber, @"^[0-9+\-\s()]{8,20}$"))
                    return OperationResult.Failure("Số điện thoại không đúng định dạng.");
            }

            if (student.Gender != null &&
                student.Gender != "Male" &&
                student.Gender != "Female" &&
                student.Gender != "Other")
            {
                return OperationResult.Failure("Giới tính chỉ được là Male, Female hoặc Other.");
            }

            if (student.Address != null && student.Address.Length > 150)
                return OperationResult.Failure("Địa chỉ tối đa 150 ký tự.");

            if (student.DateOfBirth.HasValue)
            {
                if (student.DateOfBirth.Value.Date > DateTime.Today)
                    return OperationResult.Failure("Ngày sinh không được lớn hơn ngày hiện tại.");

                if (student.DateOfBirth.Value.Year < 1900)
                    return OperationResult.Failure("Ngày sinh không hợp lệ.");
            }

            return _repository.UpdateOwnProfile(student);
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