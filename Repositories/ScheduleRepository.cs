using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduleDAO _dao;

        public ScheduleRepository(LctmsDbContext context)
        {
            _dao = new ScheduleDAO(context);
        }

        public void AddSchedule(Schedule schedule)
            => _dao.AddSchedule(schedule);

        public List<Schedule> GetAllSchedules()
            => _dao.GetAllSchedules();

        public List<Schedule> GetSchedulesByClassId(int classId)
            => _dao.GetSchedulesByClassId(classId);

        public void UpdateSchedule(Schedule schedule)
            => _dao.UpdateSchedule(schedule);

        public void DeleteSchedule(int id)
            => _dao.DeleteSchedule(id);

        public void DeleteByClassId(int classId)
            => _dao.DeleteByClassId(classId);
    }
}
