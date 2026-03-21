using BusinessObjects;
using DataAccess;
using System;
using System.Collections.Generic;

namespace Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduleDAO _dao;

        public ScheduleRepository()
        {
            _dao = new ScheduleDAO();
        }

        public ScheduleRepository(LctmsDbContext context)
        {
            _dao = new ScheduleDAO(context);
        }

        public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek)
            => _dao.GetAdminWeeklySchedule(anyDateInWeek);

        public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek)
            => _dao.GetTeacherWeeklySchedule(teacherId, anyDateInWeek);

        public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
            => _dao.GetStudentWeeklySchedule(studentId, anyDateInWeek);

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
            => _dao.GetAdminScheduleDetail(classId, dayOfWeek, slotId);

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
            => _dao.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
            => _dao.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);

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