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

        public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek, ScheduleFilterViewModel? filter = null)
        {
            return BuildWeeklySchedule(anyDateInWeek, null, null, filter);
        }

        public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek, ScheduleFilterViewModel? filter = null)
        {
            return BuildWeeklySchedule(anyDateInWeek, teacherId, null, filter);
        }

        public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
        {
            return BuildWeeklySchedule(anyDateInWeek, null, studentId, null);
        }

        public ScheduleFilterOptionsViewModel GetAdminScheduleFilterOptions(DateTime anyDateInWeek)
        {
            return BuildScheduleFilterOptions(anyDateInWeek, null);
        }

        public ScheduleFilterOptionsViewModel GetTeacherScheduleFilterOptions(int teacherId, DateTime anyDateInWeek)
        {
            return BuildScheduleFilterOptions(anyDateInWeek, teacherId);
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
                     RoomName = sc.RoomName ?? string.Empty,
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate,
                     Capacity = cl.Capacity,
                     Status = cl.Status
                 }).FirstOrDefault();

            if (detail == null) return null;

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
                     CourseCode = c.CourseCode,
                     CourseName = c.Name,
                     RoomName = sc.RoomName ?? string.Empty,
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate,
                     Capacity = cl.Capacity,
                     Status = cl.Status
                 }).FirstOrDefault();

            if (detail == null) return null;

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
            var detail =
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
                     CourseCode = c.CourseCode,
                     CourseName = c.Name,
                     TeacherName = t != null ? t.FullName : "N/A",
                     RoomName = sc.RoomName ?? string.Empty,
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,
                     StartDate = cl.StartDate,
                     EndDate = cl.EndDate,
                     Capacity = cl.Capacity,
                     Status = cl.Status
                 }).FirstOrDefault();

            if (detail == null) return null;

            detail.StudentNames =
                (from er in _context.Enrollments
                 join s in _context.Students on er.StudentId equals s.Id
                 where er.ClassId == classId && er.Status == "Approved"
                 orderby s.FullName
                 select s.FullName).ToList();

            return detail;
        }


        private ScheduleWeekViewModel BuildWeeklySchedule(
            DateTime anyDateInWeek,
            int? teacherId,
            int? studentId,
            ScheduleFilterViewModel? filter)
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
                 select new ScheduleCellViewModel
                 {
                     DayOfWeek = sc.DayOfWeek,
                     SlotId = sc.SlotId,
                     SlotName = sl.SlotName,
                     ClassId = cl.Id,
                     CourseId = cl.CourseId,
                     TeacherId = cl.TeacherId,
                     ClassCode = cl.ClassCode,
                     CourseName = c.Name,
                     TeacherName = t != null ? t.FullName : "N/A",
                     RoomName = sc.RoomName ?? string.Empty,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime
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
                    .Where(x => x.ClassId.HasValue && approvedClassIds.Contains(x.ClassId.Value))
                    .ToList();
            }

            rawSchedules = ApplyFilter(rawSchedules, filter);

            var fullCells = new List<ScheduleCellViewModel>();

            foreach (var slot in slots)
            {
                for (int day = 1; day <= 7; day++)
                {
                    var actualDate = weekStart.AddDays(day - 1);

                    var existingCells = rawSchedules
                        .Where(x =>
                            x.SlotId == slot.Id &&
                            x.DayOfWeek == day)
                        .ToList();

                    if (existingCells.Any())
                    {
                        fullCells.AddRange(existingCells);
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

        private List<ScheduleCellViewModel> ApplyFilter(
            List<ScheduleCellViewModel> source,
            ScheduleFilterViewModel? filter)
        {
            if (filter == null)
            {
                return source;
            }

            IEnumerable<ScheduleCellViewModel> query = source;

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                string keyword = filter.Keyword.Trim();
                query = query.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.ClassCode) && x.ClassCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.CourseName) && x.CourseName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.TeacherName) && x.TeacherName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(x.RoomName) && x.RoomName.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            if (filter.TeacherId.HasValue)
            {
                query = query.Where(x => x.TeacherId == filter.TeacherId.Value);
            }

            if (filter.CourseId.HasValue)
            {
                query = query.Where(x => x.CourseId == filter.CourseId.Value);
            }

            if (filter.ClassId.HasValue)
            {
                query = query.Where(x => x.ClassId == filter.ClassId.Value);
            }

            if (filter.SlotId.HasValue)
            {
                query = query.Where(x => x.SlotId == filter.SlotId.Value);
            }

            return query.ToList();
        }

        private ScheduleFilterOptionsViewModel BuildScheduleFilterOptions(DateTime anyDateInWeek, int? teacherId)
        {
            var weekStart = GetWeekStart(anyDateInWeek);
            var weekEnd = weekStart.AddDays(6);

            var query =
                from sc in _context.Schedules
                join cl in _context.Classes on sc.ClassId equals cl.Id
                join c in _context.Courses on cl.CourseId equals c.Id
                join sl in _context.Slots on sc.SlotId equals sl.Id
                join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                from t in teacherJoin.DefaultIfEmpty()
                where cl.StartDate <= weekEnd && cl.EndDate >= weekStart
                select new
                {
                    TeacherId = cl.TeacherId,
                    TeacherName = t != null ? t.FullName : "N/A",
                    CourseId = c.Id,
                    CourseName = c.Name,
                    ClassId = cl.Id,
                    cl.ClassCode,
                    SlotId = sl.Id,
                    SlotName = sl.SlotName,
                    sl.StartTime
                };

            var data = query.ToList();

            if (teacherId.HasValue)
            {
                data = data.Where(x => x.TeacherId == teacherId.Value).ToList();
            }

            return new ScheduleFilterOptionsViewModel
            {
                TeacherOptions = data
                    .Where(x => x.TeacherId.HasValue)
                    .GroupBy(x => new { Id = x.TeacherId!.Value, x.TeacherName })
                    .OrderBy(x => x.Key.TeacherName)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.Id,
                        DisplayName = x.Key.TeacherName
                    })
                    .ToList(),

                CourseOptions = data
                    .GroupBy(x => new { x.CourseId, x.CourseName })
                    .OrderBy(x => x.Key.CourseName)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.CourseId,
                        DisplayName = x.Key.CourseName
                    })
                    .ToList(),

                ClassOptions = data
                    .GroupBy(x => new { x.ClassId, x.ClassCode })
                    .OrderBy(x => x.Key.ClassCode)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.ClassId,
                        DisplayName = x.Key.ClassCode
                    })
                    .ToList(),

                SlotOptions = data
                    .GroupBy(x => new { x.SlotId, x.SlotName, x.StartTime })
                    .OrderBy(x => x.Key.StartTime)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.SlotId,
                        DisplayName = x.Key.SlotName
                    })
                    .ToList()
            };
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int day = (int)date.DayOfWeek;
            day = day == 0 ? 7 : day;
            return date.Date.AddDays(-(day - 1));
        }

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
