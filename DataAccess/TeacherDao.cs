
﻿using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class TeacherDAO
    {
        private readonly LctmsDbContext? _context;

        public TeacherDAO()
        {
        }

        public TeacherDAO(LctmsDbContext context)
        {
            _context = context;
        }

        private LctmsDbContext GetContext()
        {
            return _context ?? DbContextFactory.CreateDbContext();
        }

        public List<TeacherListItem> GetTeacherList(string? keyword = null)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            var query = db.Teachers
                .Select(t => new TeacherListItem
                {
                    Id = t.Id,
                    TeacherCode = t.TeacherCode,
                    FullName = t.FullName,
                    Gender = t.Gender,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    IsActive = t.IsActive
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();

                query = query.Where(t =>
                    t.TeacherCode.ToLower().Contains(keyword) ||
                    t.FullName.ToLower().Contains(keyword) ||
                    t.Email.ToLower().Contains(keyword) ||
                    (t.PhoneNumber != null && t.PhoneNumber.ToLower().Contains(keyword)));
            }

            return query
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public List<Teacher> GetAllTeachers()
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .AsNoTracking()
                .OrderBy(t => t.FullName)
                .ToList();
        }

        public Teacher? GetTeacherById(int id)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .AsNoTracking()
                .FirstOrDefault(t => t.Id == id);
        }

        public Teacher? GetByEmail(string email)
        {
            using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
            var db = _context ?? context!;

            return db.Teachers
                .FirstOrDefault(x => x.Email == email && x.IsActive);
        }
        public OperationResult UpdateOwnProfile(Teacher teacher)
        {
            try
            {
                using var context = _context == null ? DbContextFactory.CreateDbContext() : null;
                var db = _context ?? context!;

                var existing = db.Teachers.FirstOrDefault(x => x.Id == teacher.Id);
                if (existing == null)
                    return OperationResult.Failure("Không tìm thấy giáo viên.");

                if (!existing.IsActive)
                    return OperationResult.Failure("Tài khoản giáo viên đang bị khóa.");

                var duplicatedEmail = db.Teachers.Any(x =>
                    x.Id != teacher.Id &&
                    x.Email.ToLower() == teacher.Email.ToLower());

                if (duplicatedEmail)
                    return OperationResult.Failure("Email đã được sử dụng bởi tài khoản khác.");

                // Chỉ cho sửa thông tin cá nhân
                existing.FullName = teacher.FullName;
                existing.Email = teacher.Email;
                existing.PhoneNumber = teacher.PhoneNumber;
                existing.DateOfBirth = teacher.DateOfBirth;
                existing.Gender = teacher.Gender;
                existing.Address = teacher.Address;

                db.SaveChanges();
                return OperationResult.Success("Cập nhật hồ sơ thành công.");
            }
            catch (Exception ex)
            {
                return OperationResult.Failure($"Lỗi khi cập nhật hồ sơ giáo viên: {ex.Message}");
            }
        }
    }
}