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
            var rows = BuildFilterSourceRows(null);
            return BuildFilterOptions(rows, includeTeacher: true);
        }

        public ScheduleFilterOptionsViewModel GetTeacherScheduleFilterOptions(int teacherId, DateTime anyDateInWeek)
        {
            var rows = BuildFilterSourceRows(teacherId);
            return BuildFilterOptions(rows, includeTeacher: false);
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

        private ScheduleWeekViewModel BuildWeeklySchedule(
            DateTime anyDateInWeek,
            int? teacherId,
            int? studentId,
            ScheduleFilterViewModel? filter)
        {
            var weekStart = GetWeekStart(anyDateInWeek);
            var weekEnd = weekStart.AddDays(6);

            var rawSchedules = BuildBaseScheduleRows(anyDateInWeek, teacherId, studentId);
            rawSchedules = ApplyFilter(rawSchedules, filter);

            var slots = _context.Slots
                .OrderBy(x => x.StartTime)
                .ToList();

            if (filter?.SlotId.HasValue == true)
            {
                slots = slots
                    .Where(x => x.Id == filter.SlotId.Value)
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
        private List<ScheduleRow> BuildBaseScheduleRows(DateTime anyDateInWeek, int? teacherId, int? studentId)
        {
            var weekStart = GetWeekStart(anyDateInWeek);
            var weekEnd = weekStart.AddDays(6);

            var rawSchedules =
                (from sc in _context.Schedules
                 join cl in _context.Classes on sc.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                 from t in teacherJoin.DefaultIfEmpty()
                 where cl.StartDate <= weekEnd && cl.EndDate >= weekStart
                 select new ScheduleRow
                 {
                     DayOfWeek = (int)sc.DayOfWeek,
                     SlotId = sc.SlotId,
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,

                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,

                     CourseId = c.Id,
                     CourseCode = c.CourseCode,
                     CourseName = c.Name,

                     TeacherId = cl.TeacherId,
                     TeacherName = t != null ? t.FullName : "N/A",

                     RoomName = sc.RoomName,

                     ClassStartDate = cl.StartDate,
                     ClassEndDate = cl.EndDate
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

            return rawSchedules;
        }
        private List<ScheduleRow> BuildFilterSourceRows(int? teacherId)
        {
            var rows =
                (from sc in _context.Schedules
                 join cl in _context.Classes on sc.ClassId equals cl.Id
                 join c in _context.Courses on cl.CourseId equals c.Id
                 join sl in _context.Slots on sc.SlotId equals sl.Id
                 join t in _context.Teachers on cl.TeacherId equals t.Id into teacherJoin
                 from t in teacherJoin.DefaultIfEmpty()
                 select new ScheduleRow
                 {
                     DayOfWeek = (int)sc.DayOfWeek,
                     SlotId = sc.SlotId,
                     SlotName = sl.SlotName,
                     StartTime = sl.StartTime,
                     EndTime = sl.EndTime,

                     ClassId = cl.Id,
                     ClassCode = cl.ClassCode,

                     CourseId = c.Id,
                     CourseCode = c.CourseCode,
                     CourseName = c.Name,

                     TeacherId = cl.TeacherId,
                     TeacherName = t != null ? t.FullName : "N/A",

                     RoomName = sc.RoomName,

                     ClassStartDate = cl.StartDate,
                     ClassEndDate = cl.EndDate
                 }).ToList();

            if (teacherId.HasValue)
            {
                rows = rows
                    .Where(x => x.TeacherId == teacherId.Value)
                    .ToList();
            }

            return rows;
        }
        private List<ScheduleRow> ApplyFilter(List<ScheduleRow> rows, ScheduleFilterViewModel? filter)
        {
            if (filter == null)
            {
                return rows;
            }

            if (filter.TeacherId.HasValue)
            {
                rows = rows
                    .Where(x => x.TeacherId == filter.TeacherId.Value)
                    .ToList();
            }

            if (filter.CourseId.HasValue)
            {
                rows = rows
                    .Where(x => x.CourseId == filter.CourseId.Value)
                    .ToList();
            }

            if (filter.ClassId.HasValue)
            {
                rows = rows
                    .Where(x => x.ClassId == filter.ClassId.Value)
                    .ToList();
            }

            if (filter.SlotId.HasValue)
            {
                rows = rows
                    .Where(x => x.SlotId == filter.SlotId.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();

                rows = rows
                    .Where(x =>
                        (!string.IsNullOrWhiteSpace(x.ClassCode) && x.ClassCode.ToLower().Contains(keyword)) ||
                        (!string.IsNullOrWhiteSpace(x.CourseCode) && x.CourseCode.ToLower().Contains(keyword)) ||
                        (!string.IsNullOrWhiteSpace(x.CourseName) && x.CourseName.ToLower().Contains(keyword)) ||
                        (!string.IsNullOrWhiteSpace(x.TeacherName) && x.TeacherName.ToLower().Contains(keyword)) ||
                        (!string.IsNullOrWhiteSpace(x.RoomName) && x.RoomName.ToLower().Contains(keyword)))
                    .ToList();
            }

            return rows;
        }

        private ScheduleFilterOptionsViewModel BuildFilterOptions(List<ScheduleRow> rows, bool includeTeacher)
        {
            var result = new ScheduleFilterOptionsViewModel();

            if (includeTeacher)
            {
                result.TeacherOptions.Add(new ScheduleFilterOptionItem
                {
                    Id = null,
                    DisplayName = "All teachers"
                });

                result.TeacherOptions.AddRange(
                    rows.Where(x => x.TeacherId.HasValue)
                        .GroupBy(x => new { Id = x.TeacherId!.Value, x.TeacherName })
                        .OrderBy(x => x.Key.TeacherName)
                        .Select(x => new ScheduleFilterOptionItem
                        {
                            Id = x.Key.Id,
                            DisplayName = x.Key.TeacherName
                        })
                );
            }

            result.CourseOptions.Add(new ScheduleFilterOptionItem
            {
                Id = null,
                DisplayName = "All courses"
            });

            result.CourseOptions.AddRange(
                rows.GroupBy(x => new { x.CourseId, x.CourseCode, x.CourseName })
                    .OrderBy(x => x.Key.CourseCode)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.CourseId,
                        DisplayName = $"{x.Key.CourseCode} - {x.Key.CourseName}"
                    })
            );

            result.ClassOptions.Add(new ScheduleFilterOptionItem
            {
                Id = null,
                DisplayName = "All classes"
            });

            result.ClassOptions.AddRange(
                rows.GroupBy(x => new { x.ClassId, x.ClassCode })
                    .OrderBy(x => x.Key.ClassCode)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.ClassId,
                        DisplayName = x.Key.ClassCode
                    })
            );

            result.SlotOptions.Add(new ScheduleFilterOptionItem
            {
                Id = null,
                DisplayName = "All slots"
            });

            result.SlotOptions.AddRange(
                rows.GroupBy(x => new { x.SlotId, x.SlotName, x.StartTime, x.EndTime })
                    .OrderBy(x => x.Key.StartTime)
                    .Select(x => new ScheduleFilterOptionItem
                    {
                        Id = x.Key.SlotId,
                        DisplayName = $"{x.Key.SlotName} ({x.Key.StartTime:hh\\:mm}-{x.Key.EndTime:hh\\:mm})"
                    })
            );

            return result;
        }
        private DateTime GetWeekStart(DateTime date)
        {
            int day = (int)date.DayOfWeek;
            day = day == 0 ? 7 : day; // Sunday => 7
            return date.Date.AddDays(-(day - 1));
        }
        private sealed class ScheduleRow
        {
            public int DayOfWeek { get; set; }
            public int SlotId { get; set; }
            public string SlotName { get; set; } = string.Empty;
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }

            public int ClassId { get; set; }
            public string ClassCode { get; set; } = string.Empty;

            public int CourseId { get; set; }
            public string CourseCode { get; set; } = string.Empty;
            public string CourseName { get; set; } = string.Empty;

            public int? TeacherId { get; set; }
            public string TeacherName { get; set; } = "N/A";

            public string? RoomName { get; set; }

            public DateTime ClassStartDate { get; set; }
            public DateTime ClassEndDate { get; set; }
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