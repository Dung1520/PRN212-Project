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
        private readonly IScheduleRepository _repo;

        public ScheduleService(IScheduleRepository repo)
        {
            _repo = repo;
        }

        public void AddSchedule(Schedule schedule)
        {
            // ❌ check trùng lịch trong DB
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
        {
            _repo.UpdateSchedule(schedule);
        }

        public void DeleteSchedule(int id)
        {
            _repo.DeleteSchedule(id);
        }

        public void DeleteByClassId(int classId)
        {
            _repo.DeleteByClassId(classId);
        }
    }
}
