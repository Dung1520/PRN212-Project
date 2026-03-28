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

        public OperationResult RegisterStudent(StudentRegistrationRequest request)
        {
            if (request == null)
                return OperationResult.Failure("Dữ liệu đăng ký không hợp lệ.");

            request.Username = request.Username?.Trim() ?? string.Empty;
            request.Email = request.Email?.Trim() ?? string.Empty;
            request.Password = request.Password?.Trim() ?? string.Empty;
            request.ConfirmPassword = request.ConfirmPassword?.Trim() ?? string.Empty;
            request.FullName = request.FullName?.Trim() ?? string.Empty;
            request.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            request.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
            request.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

            if (string.IsNullOrWhiteSpace(request.Username))
                return OperationResult.Failure("Username không được để trống.");

            if (request.Username.Length < 4 || request.Username.Length > 50)
                return OperationResult.Failure("Username phải từ 4 đến 50 ký tự.");

            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9._]+$"))
                return OperationResult.Failure("Username chỉ được chứa chữ cái, số, dấu chấm hoặc dấu gạch dưới.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                return OperationResult.Failure("Họ và tên không được để trống.");

            if (request.FullName.Length > 100)
                return OperationResult.Failure("Họ và tên tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return OperationResult.Failure("Email không được để trống.");

            if (request.Email.Length > 100)
                return OperationResult.Failure("Email tối đa 100 ký tự.");

            if (!IsValidEmail(request.Email))
                return OperationResult.Failure("Email không đúng định dạng.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return OperationResult.Failure("Mật khẩu không được để trống.");

            if (request.Password.Length < 6 || request.Password.Length > 50)
                return OperationResult.Failure("Mật khẩu phải từ 6 đến 50 ký tự.");

            if (request.Password != request.ConfirmPassword)
                return OperationResult.Failure("Xác nhận mật khẩu không khớp.");

            if (request.PhoneNumber != null)
            {
                if (request.PhoneNumber.Length > 20)
                    return OperationResult.Failure("Số điện thoại tối đa 20 ký tự.");

                if (!Regex.IsMatch(request.PhoneNumber, @"^[0-9+\-\s()]{8,20}$"))
                    return OperationResult.Failure("Số điện thoại không đúng định dạng.");
            }

            if (request.Gender != null &&
                request.Gender != "Male" &&
                request.Gender != "Female" &&
                request.Gender != "Other")
            {
                return OperationResult.Failure("Giới tính chỉ được là Male, Female hoặc Other.");
            }

            if (request.Address != null && request.Address.Length > 150)
                return OperationResult.Failure("Địa chỉ tối đa 150 ký tự.");

            if (request.DateOfBirth.HasValue)
            {
                if (request.DateOfBirth.Value.Date > DateTime.Today)
                    return OperationResult.Failure("Ngày sinh không được lớn hơn ngày hiện tại.");

                if (request.DateOfBirth.Value.Year < 1900)
                    return OperationResult.Failure("Ngày sinh không hợp lệ.");
            }

            var student = new Student
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                IsActive = true
            };

            return _repository.RegisterStudent(student);
        }

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