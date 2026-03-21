using BusinessObjects;
using Microsoft.Extensions.Configuration;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAuthService
    {
        LoginResult Login(string email, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IAdminRepository _adminRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IStudentRepository _studentRepo;

        private readonly string _defaultAdminEmail;
        private readonly string _defaultAdminPassword;

        public AuthService()
        {
            _adminRepo = new AdminRepository();
            _teacherRepo = new TeacherRepository();
            _studentRepo = new StudentRepository();

            // Load appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _defaultAdminEmail = config["DefaultAdmin:Email"]!;
            _defaultAdminPassword = config["DefaultAdmin:Password"]!;
        }

        public LoginResult Login(string email, string password)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = "Email và password không được để trống"
                };
            }

            // 1. Check default admin (appsettings)
            if (email == _defaultAdminEmail && password == _defaultAdminPassword)
            {
                return new LoginResult
                {
                    IsSuccess = true,
                    Role = "Admin",
                    Message = "Login as default admin"
                };
            }

            // 2. Check Admin DB
            var admin = _adminRepo.GetByEmail(email);
            if (admin != null && admin.Password == password)
            {
                return new LoginResult
                {
                    IsSuccess = true,
                    Role = "Admin",
                    User = admin
                };
            }

            // 3. Check Teacher
            var teacher = _teacherRepo.GetByEmail(email);
            if (teacher != null && teacher.Password == password)
            {
                return new LoginResult
                {
                    IsSuccess = true,
                    Role = "Teacher",
                    User = teacher
                };
            }

            // 4. Check Student
            var student = _studentRepo.GetByEmail(email);
            if (student != null && student.Password == password)
            {
                return new LoginResult
                {
                    IsSuccess = true,
                    Role = "Student",
                    User = student
                };
            }

            // Fail
            return new LoginResult
            {
                IsSuccess = false,
                Message = "Sai email hoặc password"
            };
        }
    }
}
