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

        public ScheduleService()
        {
            _scheduleRepository = new ScheduleRepository();
        }

        public ScheduleWeekViewModel GetWeeklySchedule(int currentUserId, string role, DateTime anyDateInWeek)
        {
            if (role == "Admin")
            {
                return _scheduleRepository.GetAdminWeeklySchedule(anyDateInWeek);
            }

            if (role == "Teacher")
            {
                return _scheduleRepository.GetTeacherWeeklySchedule(currentUserId, anyDateInWeek);
            }

            return _scheduleRepository.GetStudentWeeklySchedule(currentUserId, anyDateInWeek);
        }

        public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
        {
            return _scheduleRepository.GetAdminScheduleDetail(classId, dayOfWeek, slotId);
        }

        public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleRepository.GetTeacherScheduleDetail(teacherId, classId, dayOfWeek, slotId);
        }

        public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
        {
            return _scheduleRepository.GetStudentScheduleDetail(studentId, classId, dayOfWeek, slotId);
        }
    }
}
