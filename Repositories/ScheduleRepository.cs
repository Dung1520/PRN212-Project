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
        private readonly ScheduleDAO _dao;

        public ScheduleRepository()
        public ScheduleRepository(LctmsDbContext context)
        {
            _scheduleDao = new ScheduleDAO();
            _dao = new ScheduleDAO(context);
        }

        public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek)
        {
            return _scheduleDao.GetAdminWeeklySchedule(anyDateInWeek);
        }
        public void AddSchedule(Schedule schedule)
            => _dao.AddSchedule(schedule);

        public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek)
        {
            return _scheduleDao.GetTeacherWeeklySchedule(teacherId, anyDateInWeek);
        }
        public List<Schedule> GetAllSchedules()
            => _dao.GetAllSchedules();

        public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
        {
            return _scheduleDao.GetStudentWeeklySchedule(studentId, anyDateInWeek);
        }
        public List<Schedule> GetSchedulesByClassId(int classId)
            => _dao.GetSchedulesByClassId(classId);

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetAdminScheduleDetail(classId, dayOfWeek, slotId);
        }
        public void UpdateSchedule(Schedule schedule)
            => _dao.UpdateSchedule(schedule);

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);
        }
        public void DeleteSchedule(int id)
            => _dao.DeleteSchedule(id);

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleDao.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);
        }
        public void DeleteByClassId(int classId)
            => _dao.DeleteByClassId(classId);
    }
}
