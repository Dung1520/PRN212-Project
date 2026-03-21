using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ScheduleDAO
    {
        private readonly LctmsDbContext _context;

        public ScheduleDAO(LctmsDbContext context)
        {
            _context = context;
        }

        // CREATE
        public void AddSchedule(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            _context.SaveChanges();
        }

        // READ ALL
        public List<Schedule> GetAllSchedules()
        {
            return _context.Schedules.ToList();
        }

        // READ BY CLASS
        public List<Schedule> GetSchedulesByClassId(int classId)
        {
            return _context.Schedules
                           .Where(s => s.ClassId == classId)
                           .ToList();
        }

        // UPDATE
        public void UpdateSchedule(Schedule schedule)
        {
            var existing = _context.Schedules
                                   .FirstOrDefault(s => s.Id == schedule.Id);

            if (existing != null)
            {
                existing.DayOfWeek = schedule.DayOfWeek;
                existing.SlotId = schedule.SlotId;
                existing.RoomName = schedule.RoomName;

                _context.SaveChanges();
            }
        }

        // DELETE
        public void DeleteSchedule(int id)
        {
            var schedule = _context.Schedules
                                   .FirstOrDefault(s => s.Id == id);

            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                _context.SaveChanges();
            }
        }

        // DELETE BY CLASS (rất quan trọng)
        public void DeleteByClassId(int classId)
        {
            var schedules = _context.Schedules
                                    .Where(s => s.ClassId == classId)
                                    .ToList();

            _context.Schedules.RemoveRange(schedules);
            _context.SaveChanges();
        }
    }
}
