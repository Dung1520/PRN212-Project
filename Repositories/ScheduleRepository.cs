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
        private readonly ScheduleDAO _scheduleDao;

        public ScheduleRepository()
        {
            _scheduleDao = new ScheduleDAO();
        }

        public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek)
        {
            return _scheduleDao.GetAdminWeeklySchedule(anyDateInWeek);
        }

        public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek)
        {
            return _scheduleDao.GetTeacherWeeklySchedule(teacherId, anyDateInWeek);
        }

        public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
        {
            return _scheduleDao.GetStudentWeeklySchedule(studentId, anyDateInWeek);
        }

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetAdminScheduleDetail(classId, dayOfWeek, slotId);
        }

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);
        }

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);
        }
    }
}
