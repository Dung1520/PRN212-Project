using BusinessObjects;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ClassService : IClassService
    {
        private readonly LctmsDbContext _context;
        private readonly IClassRepository _repo;

        public ClassService(IClassRepository repo, LctmsDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        public void AddClass(Class c, List<Schedule> schedules)
        {
            if (schedules
                .GroupBy(s => new { s.DayOfWeek, s.SlotId })
                .Any(g => g.Count() > 1))
            {
                throw new Exception("Duplicate schedule (Day + Slot)");
            }

            foreach (var s in schedules)
            {
                // Teacher conflict
                if (c.TeacherId != null)
                {
                    bool teacherConflict = _context.Schedules
                        .Include(x => x.Class)
                        .Any(x =>
                            x.DayOfWeek == s.DayOfWeek &&
                            x.SlotId == s.SlotId &&
                            x.Class.TeacherId == c.TeacherId &&
                            x.ClassId != c.Id
                        );

                    if (teacherConflict)
                        throw new Exception("Giáo viên bị trùng lịch!");
                }
            }

            foreach (var s in schedules)
            {
                bool isConflict = _context.Schedules.Any(x =>
                    x.DayOfWeek == s.DayOfWeek &&
                    x.SlotId == s.SlotId &&
                    x.RoomName == s.RoomName
                );

                if (isConflict)
                {
                    throw new Exception(
                        $"Phòng {s.RoomName} đã có lớp"
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(c.ClassCode))
                throw new Exception("ClassCode is required");

            if (schedules.Any(s => string.IsNullOrEmpty(s.RoomName)))
                throw new Exception("Room is required");

            if (c.CourseId <= 0)
                throw new Exception("CourseId invalid");

            if (_repo.GetAllClasses().Any(x => x.ClassCode == c.ClassCode))
                throw new Exception("ClassCode already exists");

            if (c.Capacity <= 0)
                throw new Exception("Capacity must be > 0");

            if (c.StartDate > c.EndDate)
                throw new Exception("StartDate must be before EndDate");

            if (schedules == null || schedules.Count == 0)
                throw new Exception("Schedule must not be empty");

            using var tran = _context.Database.BeginTransaction();

            try
            {
                // 1. Add class
                c.CreatedAt = DateTime.Now;
                c.Status = string.IsNullOrEmpty(c.Status) ? "Open" : c.Status;

                _context.Classes.Add(c);
                _context.SaveChanges();

                // 2. Add schedules
                foreach (var s in schedules)
                {
                    s.ClassId = c.Id;
                    _context.Schedules.Add(s);
                }

                _context.SaveChanges();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void DeleteClass(int id)
        {
            throw new NotImplementedException();
        }

        public List<Class> GetAllClasses()
        {
            return _repo.GetAllClasses();
        }

        public Class? GetClassById(int id)
        {
            return _repo.GetClassById(id);
        }

        public void UpdateClass(Class c, List<Schedule> schedules)
        {
            using var tran = _context.Database.BeginTransaction();
            foreach (var s in schedules)
            {
                // Teacher conflict
                if (c.TeacherId != null)
                {
                    bool teacherConflict = _context.Schedules
                        .Include(x => x.Class)
                        .Any(x =>
                            x.DayOfWeek == s.DayOfWeek &&
                            x.SlotId == s.SlotId &&
                            x.Class.TeacherId == c.TeacherId &&
                            x.ClassId != c.Id
                        );

                    if (teacherConflict)
                        throw new Exception("Giáo viên bị trùng lịch!");
                }
            }

            foreach (var s in schedules)
            {
                bool isConflict = _context.Schedules.Any(x =>
                    x.ClassId != c.Id && // 🔥 loại chính nó
                    x.DayOfWeek == s.DayOfWeek &&
                    x.SlotId == s.SlotId &&
                    x.RoomName == s.RoomName
                );

                if (isConflict)
                {
                    throw new Exception(
                        $"Phòng {s.RoomName} đã có lớp"
                    );
                }
            }

            if (_repo.GetAllClasses().Any(x => x.ClassCode == c.ClassCode && x.Id != c.Id))
                throw new Exception("ClassCode already exists");

            try
            {
                var existing = _context.Classes.FirstOrDefault(x => x.Id == c.Id);
                if (existing == null)
                    throw new Exception("Class not found");

                // update class
                existing.ClassCode = c.ClassCode;
                existing.Capacity = c.Capacity;
                existing.CourseId = c.CourseId;
                existing.TeacherId = c.TeacherId;
                existing.StartDate = c.StartDate;
                existing.EndDate = c.EndDate;
                existing.Status = c.Status;

                // ❌ xóa schedule cũ
                var oldSchedules = _context.Schedules.Where(s => s.ClassId == c.Id).ToList();
                _context.Schedules.RemoveRange(oldSchedules);

                // ➕ thêm lại schedule mới
                foreach (var s in schedules)
                {
                    var newSchedule = new Schedule
                    {
                        ClassId = c.Id,
                        DayOfWeek = s.DayOfWeek,
                        SlotId = s.SlotId,
                        RoomName = s.RoomName
                    };

                    _context.Schedules.Add(newSchedule);
                }

                _context.SaveChanges();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
