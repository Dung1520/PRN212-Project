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
        ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek);
        ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek);
        ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek);

        AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId);
        TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId);
        StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId);
        void AddSchedule(Schedule schedule);
        List<Schedule> GetAllSchedules();
        List<Schedule> GetSchedulesByClassId(int classId);
        void UpdateSchedule(Schedule schedule);
        void DeleteSchedule(int id);
        void DeleteByClassId(int classId);
    }
}
