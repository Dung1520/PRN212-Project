using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repo;

        public ScheduleService()
        {
            _repo = new ScheduleRepository();
        }

        public ScheduleService(IScheduleRepository repo)
        {
            _repo = repo;
        }

        public ScheduleWeekViewModel GetWeeklySchedule(
            int currentUserId,
            string role,
            DateTime anyDateInWeek,
            ScheduleFilterViewModel? filter = null)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return _repo.GetAdminWeeklySchedule(anyDateInWeek, filter);
            }

            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                return _repo.GetTeacherWeeklySchedule(currentUserId, anyDateInWeek, filter);
            }

            return _repo.GetStudentWeeklySchedule(currentUserId, anyDateInWeek);
        }

        public ScheduleFilterOptionsViewModel GetScheduleFilterOptions(
            int currentUserId,
            string role,
            DateTime anyDateInWeek)
        {
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return _repo.GetAdminScheduleFilterOptions(anyDateInWeek);
            }

            if (string.Equals(role, "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                return _repo.GetTeacherScheduleFilterOptions(currentUserId, anyDateInWeek);
            }

            return new ScheduleFilterOptionsViewModel();
        }


        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
            => _repo.GetAdminScheduleDetail(classId, dayOfWeek, slotId);

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
            => _repo.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
            => _repo.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);

        public void AddSchedule(Schedule schedule)
        {
            var existing = _repo.GetSchedulesByClassId(schedule.ClassId);

            if (existing.Any(s =>
                s.DayOfWeek == schedule.DayOfWeek &&
                s.SlotId == schedule.SlotId))
            {
                throw new Exception("Trùng lịch học!");
            }

            _repo.AddSchedule(schedule);
        }

        public List<Schedule> GetAllSchedules()
            => _repo.GetAllSchedules();

        public List<Schedule> GetSchedulesByClassId(int classId)
            => _repo.GetSchedulesByClassId(classId);

        public void UpdateSchedule(Schedule schedule)
            => _repo.UpdateSchedule(schedule);

        public void DeleteSchedule(int id)
            => _repo.DeleteSchedule(id);

        public void DeleteByClassId(int classId)
            => _repo.DeleteByClassId(classId);
    }
}