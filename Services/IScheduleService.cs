using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IScheduleService
    {
        ScheduleWeekViewModel GetWeeklySchedule(int currentUserId, string role, DateTime anyDateInWeek);

        AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId);
        TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId);
        StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId);
    }
}
