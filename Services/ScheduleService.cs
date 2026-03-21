using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleRepository _repo;

        public ScheduleService()
        public ScheduleService(IScheduleRepository repo)
        {
            _scheduleRepository = new ScheduleRepository();
            _repo = repo;
        }

        public ScheduleWeekViewModel GetWeeklySchedule(int currentUserId, string role, DateTime anyDateInWeek)
        public void AddSchedule(Schedule schedule)
        {
            if (role == "Admin")
            // ❌ check trùng lịch trong DB
            var existing = _repo.GetSchedulesByClassId(schedule.ClassId);

            if (existing.Any(s =>
                s.DayOfWeek == schedule.DayOfWeek &&
                s.SlotId == schedule.SlotId))
            {
                return _scheduleRepository.GetAdminWeeklySchedule(anyDateInWeek);
                throw new Exception("Trùng lịch học!");
            }

            if (role == "Teacher")
            {
                return _scheduleRepository.GetTeacherWeeklySchedule(currentUserId, anyDateInWeek);
            _repo.AddSchedule(schedule);
            }

            return _scheduleRepository.GetStudentWeeklySchedule(currentUserId, anyDateInWeek);
        }
        public List<Schedule> GetAllSchedules()
            => _repo.GetAllSchedules();

        public List<Schedule> GetSchedulesByClassId(int classId)
            => _repo.GetSchedulesByClassId(classId);

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
        public void UpdateSchedule(Schedule schedule)
        {
            return _scheduleRepository.GetAdminScheduleDetail(classId, dayOfWeek, slotId);
            _repo.UpdateSchedule(schedule);
        }

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
        public void DeleteSchedule(int id)
        {
            return _scheduleRepository.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);
            _repo.DeleteSchedule(id);
        }

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
        public void DeleteByClassId(int classId)
        {
            return _scheduleRepository.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);
            _repo.DeleteByClassId(classId);
        }
    }
}
