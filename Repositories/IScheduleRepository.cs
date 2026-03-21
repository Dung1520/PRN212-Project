using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IScheduleRepository
    {
        void AddSchedule(Schedule schedule);
        List<Schedule> GetAllSchedules();
        List<Schedule> GetSchedulesByClassId(int classId);
        void UpdateSchedule(Schedule schedule);
        void DeleteSchedule(int id);
        void DeleteByClassId(int classId);
    }
}
