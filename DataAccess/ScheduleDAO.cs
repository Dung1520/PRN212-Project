using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class ScheduleDAO
    {
        private readonly LctmsDbContext _context;

        public ScheduleDAO()
        {
            _context = new LctmsDbContext();
        }

        public ScheduleDAO(LctmsDbContext context)
        {
            _context = context;
        }

        // =========================
        // WEEKLY SCHEDULE
        // =========================
        public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek)
        {
            return BuildWeeklySchedule(anyDateInWeek, null, null);
        }

        public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek)
        {
            return BuildWeeklySchedule(anyDateInWeek, teacherId, null);
        }

        public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
        {
            return BuildWeeklySchedule(anyDateInWeek, null, studentId);
        }

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
        {
            var detail =
                (from sc in _context.Schedules
                 join cl in _context.Classes on sc.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                 from t in teacherJoin.DefaultIfEmpty()
                 where sc.ClassId == classId
                       && sc.DayOfWeek == dayOfWeek
                       && sc.SlotId == slotId
                 select new AdminScheduleDetailViewModel
                 {
                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,
                     CourseCode = c.CourseCode,
                     CourseName = c.Name,
                     TeacherName = t != null ? t.FullName : "N/A",
                     RoomName = sc.RoomName ?? "",
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate,
                     Capacity = cl.Capacity,
                     Status = cl.Status
                 }).FirstOrDefault();

            if (detail == null)
            {
                return null;
            }

            detail.StudentNames =
                (from e in _context.Enrollments
                 join s in _context.Students on e.StudentId equals s.Id
                 where e.ClassId == classId && e.Status == "Approved"
                 orderby s.FullName
                 select s.FullName).ToList();

            return detail;
        }

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
        {
            var detail =
                (from sc in _context.Schedules
                 join cl in _context.Classes on sc.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 where sc.ClassId == classId
                       && sc.DayOfWeek == dayOfWeek
                       && sc.SlotId == slotId
                       && cl.TeacherId == teacherId
                 select new TeacherScheduleDetailViewModel
                 {
                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,
                     CourseName = c.Name,
                     RoomName = sc.RoomName ?? "",
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate
                 }).FirstOrDefault();

            if (detail == null)
            {
                return null;
            }

            detail.StudentNames =
                (from e in _context.Enrollments
                 join s in _context.Students on e.StudentId equals s.Id
                 where e.ClassId == classId && e.Status == "Approved"
                 orderby s.FullName
                 select s.FullName).ToList();

            return detail;
        }

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
        {
            return
                (from e in _context.Enrollments
                 join cl in _context.Classes on e.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sc in _context.Schedules on cl.Id equals sc.ClassId
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                 from t in teacherJoin.DefaultIfEmpty()
                 where e.StudentId == studentId
                       && e.Status == "Approved"
                       && cl.Id == classId
                       && sc.DayOfWeek == dayOfWeek
                       && sc.SlotId == slotId
                 select new StudentScheduleDetailViewModel
                 {
                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,
                     CourseName = c.Name,
                     TeacherName = t != null ? t.FullName : "N/A",
                     RoomName = sc.RoomName ?? "",
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate
                 }).FirstOrDefault();
        }

        private ScheduleWeekViewModel BuildWeeklySchedule(DateTime anyDateInWeek, int? teacherId, int? studentId)
        {
            var weekStart = GetWeekStart(anyDateInWeek);
            var weekEnd = weekStart.AddDays(6);

            var slots = _context.Slots
                .OrderBy(x => x.StartTime)
                .ToList();

            var rawSchedules =
                (from sc in _context.Schedules
                 join cl in _context.Classes on sc.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                 from t in teacherJoin.DefaultIfEmpty()
                 where cl.StartDate <= weekEnd && cl.EndDate >= weekStart
                 select new
                 {
                     DayOfWeek = (int)sc.DayOfWeek,
                     sc.SlotId,
                     RoomName = sc.RoomName,
                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,
                     CourseName = c.Name,
                     TeacherName = t != null ? t.FullName : "N/A",
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     ClassStartDate = cl.StartDate,
                     ClassEndDate = cl.EndDate,
                     TeacherId = cl.TeacherId
                 }).ToList();

            if (teacherId.HasValue)
            {
                rawSchedules = rawSchedules
                    .Where(x => x.TeacherId == teacherId.Value)
                    .ToList();
            }

            if (studentId.HasValue)
            {
                var approvedClassIds = _context.Enrollments
                    .Where(e => e.StudentId == studentId.Value && e.Status == "Approved")
                    .Select(e => e.ClassId)
                    .ToHashSet();

                rawSchedules = rawSchedules
                    .Where(x => approvedClassIds.Contains(x.ClassId))
                    .ToList();
            }

            var fullCells = new List<ScheduleCellViewModel>();

            foreach (var slot in slots)
            {
                for (int day = 1; day <= 7; day++)
                {
                    var actualDate = weekStart.AddDays(day - 1);

                    var existings = rawSchedules
                        .Where(x =>
                            x.SlotId == slot.Id &&
                            x.DayOfWeek == day &&
                            actualDate.Date >= x.ClassStartDate.Date &&
                            actualDate.Date <= x.ClassEndDate.Date)
                        .ToList();

                    if (existings.Any())
                    {
                        foreach (var existing in existings)
                        {
                            fullCells.Add(new ScheduleCellViewModel
                            {
                                DayOfWeek = day,
                                SlotId = existing.SlotId,
                                SlotName = existing.SlotName,
                                ClassId = existing.ClassId,
                                ClassCode = existing.ClassCode,
                                CourseName = existing.CourseName,
                                TeacherName = existing.TeacherName,
                                RoomName = existing.RoomName ?? "",
                                StartTime = existing.StartTime,
                                EndTime = existing.EndTime
                            });
                        }
                    }
                    else
                    {
                        fullCells.Add(new ScheduleCellViewModel
                        {
                            DayOfWeek = day,
                            SlotId = slot.Id,
                            SlotName = slot.SlotName,
                            StartTime = slot.StartTime,
                            EndTime = slot.EndTime
                        });
                    }
                }
            }

            return new ScheduleWeekViewModel
            {
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                Cells = fullCells
            };
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int day = (int)date.DayOfWeek;
            day = day == 0 ? 7 : day; // Sunday => 7
            return date.Date.AddDays(-(day - 1));
        }

        // =========================
        // CRUD
        // =========================
        public void AddSchedule(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            _context.SaveChanges();
        }

        public List<Schedule> GetAllSchedules()
        {
            return _context.Schedules.ToList();
        }

        public List<Schedule> GetSchedulesByClassId(int classId)
        {
            return _context.Schedules
                .Where(s => s.ClassId == classId)
                .ToList();
        }

        public void UpdateSchedule(Schedule schedule)
        {
            var existing = _context.Schedules.FirstOrDefault(s => s.Id == schedule.Id);

            if (existing != null)
            {
                existing.ClassId = schedule.ClassId;
                existing.DayOfWeek = schedule.DayOfWeek;
                existing.SlotId = schedule.SlotId;
                existing.RoomName = schedule.RoomName;

                _context.SaveChanges();
            }
        }

        public void DeleteSchedule(int id)
        {
            var schedule = _context.Schedules.FirstOrDefault(s => s.Id == id);

            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                _context.SaveChanges();
            }
        }

        public void DeleteByClassId(int classId)
        {
            var schedules = _context.Schedules
                .Where(s => s.ClassId == classId)
                .ToList();

            if (schedules.Any())
            {
                _context.Schedules.RemoveRange(schedules);
                _context.SaveChanges();
            }
        }
    }
}