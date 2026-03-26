using BusinessObjects;
using DataAccess;

namespace Services
{
    public class AuthService : IAuthService
    {
        public LoginUser? Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
                return null;

            usernameOrEmail = usernameOrEmail.Trim();
            password = password.Trim();

            // 1) Login default admin from appsettings.json
            var defaultAdmin = AppSettingsHelper.GetDefaultAdmin();

            if (!string.IsNullOrWhiteSpace(defaultAdmin.Email) &&
                !string.IsNullOrWhiteSpace(defaultAdmin.Password) &&
                string.Equals(usernameOrEmail, defaultAdmin.Email, StringComparison.OrdinalIgnoreCase) &&
                password == defaultAdmin.Password)
            {
                return new LoginUser
                {
                    UserId = 0, // Admin mặc định không nhất thiết phải có record trong DB
                    Role = "Admin",
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = defaultAdmin.Email,
                    IsDefaultAdmin = true
                };
            }

            // 2) Login Teacher from database
            using var context = DbContextFactory.CreateDbContext();

            var teacher = context.Teachers
                .Where(x => x.IsActive &&
                            (x.Username == usernameOrEmail || x.Email == usernameOrEmail) &&
                            x.Password == password)
                .Select(x => new LoginUser
                {
                    UserId = x.Id,
                    Role = "Teacher",
                    Username = x.Username,
                    FullName = x.FullName,
                    Email = x.Email,
                    IsDefaultAdmin = false
                })
                .FirstOrDefault();

            if (teacher != null)
                return teacher;

            // 3) Login Student from database
            var student = context.Students
                .Where(x => x.IsActive &&
                            (x.Username == usernameOrEmail || x.Email == usernameOrEmail) &&
                            x.Password == password)
                .Select(x => new LoginUser
                {
                    UserId = x.Id,
                    Role = "Student",
                    Username = x.Username,
                    FullName = x.FullName,
                    Email = x.Email,
                    IsDefaultAdmin = false
                })
                .FirstOrDefault();

            if (student != null)
                return student;

            return null;
        }
    }
}