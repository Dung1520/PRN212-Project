using BusinessObjects;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class ScheduleServiceTests
    {
        [Fact]
        public void GetWeeklySchedule_Admin_ShouldCallAdminRepositoryMethod()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var expected = new ScheduleWeekViewModel
            {
                WeekStartDate = new DateTime(2026, 3, 23),
                WeekEndDate = new DateTime(2026, 3, 29)
            };

            fakeRepo.AdminWeeklyScheduleResult = expected;

            var filter = new ScheduleFilterViewModel
            {
                Keyword = "C#"
            };

            var result = service.GetWeeklySchedule(1, "Admin", new DateTime(2026, 3, 25), filter);

            Assert.Same(expected, result);
            Assert.Equal(1, fakeRepo.GetAdminWeeklyScheduleCalledCount);
            Assert.Equal(0, fakeRepo.GetTeacherWeeklyScheduleCalledCount);
            Assert.Equal(0, fakeRepo.GetStudentWeeklyScheduleCalledCount);
        }

        [Fact]
        public void GetWeeklySchedule_Teacher_ShouldCallTeacherRepositoryMethod()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var expected = new ScheduleWeekViewModel();
            fakeRepo.TeacherWeeklyScheduleResult = expected;

            var result = service.GetWeeklySchedule(99, "Teacher", new DateTime(2026, 3, 25));

            Assert.Same(expected, result);
            Assert.Equal(0, fakeRepo.GetAdminWeeklyScheduleCalledCount);
            Assert.Equal(1, fakeRepo.GetTeacherWeeklyScheduleCalledCount);
            Assert.Equal(0, fakeRepo.GetStudentWeeklyScheduleCalledCount);
            Assert.Equal(99, fakeRepo.LastTeacherIdForWeeklySchedule);
        }

        [Fact]
        public void GetWeeklySchedule_Student_ShouldCallStudentRepositoryMethod()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var expected = new ScheduleWeekViewModel();
            fakeRepo.StudentWeeklyScheduleResult = expected;

            var result = service.GetWeeklySchedule(123, "Student", new DateTime(2026, 3, 25));

            Assert.Same(expected, result);
            Assert.Equal(0, fakeRepo.GetAdminWeeklyScheduleCalledCount);
            Assert.Equal(0, fakeRepo.GetTeacherWeeklyScheduleCalledCount);
            Assert.Equal(1, fakeRepo.GetStudentWeeklyScheduleCalledCount);
            Assert.Equal(123, fakeRepo.LastStudentIdForWeeklySchedule);
        }

        [Fact]
        public void GetScheduleFilterOptions_Admin_ShouldCallAdminRepositoryMethod()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var expected = new ScheduleFilterOptionsViewModel
            {
                TeacherOptions = new List<ScheduleFilterOptionItem>
                {
                    new ScheduleFilterOptionItem { Id = 1, DisplayName = "Teacher A" }
                }
            };

            fakeRepo.AdminFilterOptionsResult = expected;

            var result = service.GetScheduleFilterOptions(1, "Admin", new DateTime(2026, 3, 25));

            Assert.Same(expected, result);
            Assert.Equal(1, fakeRepo.GetAdminFilterOptionsCalledCount);
            Assert.Equal(0, fakeRepo.GetTeacherFilterOptionsCalledCount);
        }

        [Fact]
        public void GetScheduleFilterOptions_Teacher_ShouldCallTeacherRepositoryMethod()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var expected = new ScheduleFilterOptionsViewModel
            {
                ClassOptions = new List<ScheduleFilterOptionItem>
                {
                    new ScheduleFilterOptionItem { Id = 10, DisplayName = "Class A" }
                }
            };

            fakeRepo.TeacherFilterOptionsResult = expected;

            var result = service.GetScheduleFilterOptions(50, "Teacher", new DateTime(2026, 3, 25));

            Assert.Same(expected, result);
            Assert.Equal(0, fakeRepo.GetAdminFilterOptionsCalledCount);
            Assert.Equal(1, fakeRepo.GetTeacherFilterOptionsCalledCount);
            Assert.Equal(50, fakeRepo.LastTeacherIdForFilterOptions);
        }

        [Fact]
        public void GetScheduleFilterOptions_Student_ShouldReturnEmptyOptions()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var result = service.GetScheduleFilterOptions(200, "Student", new DateTime(2026, 3, 25));

            Assert.NotNull(result);
            Assert.Empty(result.TeacherOptions);
            Assert.Empty(result.CourseOptions);
            Assert.Empty(result.ClassOptions);
            Assert.Empty(result.SlotOptions);
            Assert.Equal(0, fakeRepo.GetAdminFilterOptionsCalledCount);
            Assert.Equal(0, fakeRepo.GetTeacherFilterOptionsCalledCount);
        }

        [Fact]
        public void AddSchedule_WhenDuplicateDayAndSlot_ShouldThrowException()
        {
            var fakeRepo = new FakeScheduleRepository();
            fakeRepo.SchedulesByClassIdResult = new List<Schedule>
            {
                new Schedule
                {
                    Id = 1,
                    ClassId = 10,
                    DayOfWeek = 2,
                    SlotId = 1,
                    RoomName = "A101"
                }
            };

            var service = new ScheduleService(fakeRepo);

            var newSchedule = new Schedule
            {
                ClassId = 10,
                DayOfWeek = 2,
                SlotId = 1,
                RoomName = "A102"
            };

            var ex = Assert.Throws<Exception>(() => service.AddSchedule(newSchedule));

            Assert.Equal("Trùng lịch học!", ex.Message);
            Assert.Equal(0, fakeRepo.AddScheduleCalledCount);
        }

        [Fact]
        public void AddSchedule_WhenNoDuplicate_ShouldCallRepositoryAdd()
        {
            var fakeRepo = new FakeScheduleRepository();
            fakeRepo.SchedulesByClassIdResult = new List<Schedule>();

            var service = new ScheduleService(fakeRepo);

            var newSchedule = new Schedule
            {
                ClassId = 10,
                DayOfWeek = 3,
                SlotId = 2,
                RoomName = "B201"
            };

            service.AddSchedule(newSchedule);

            Assert.Equal(1, fakeRepo.AddScheduleCalledCount);
            Assert.Same(newSchedule, fakeRepo.LastAddedSchedule);
        }

        [Fact]
        public void GetAllSchedules_ShouldReturnRepositoryData()
        {
            var fakeRepo = new FakeScheduleRepository();
            var expected = new List<Schedule>
            {
                new Schedule { Id = 1, ClassId = 10, DayOfWeek = 2, SlotId = 1, RoomName = "A101" }
            };

            fakeRepo.AllSchedulesResult = expected;

            var service = new ScheduleService(fakeRepo);

            var result = service.GetAllSchedules();

            Assert.Same(expected, result);
        }

        [Fact]
        public void GetSchedulesByClassId_ShouldReturnRepositoryData()
        {
            var fakeRepo = new FakeScheduleRepository();
            var expected = new List<Schedule>
            {
                new Schedule { Id = 2, ClassId = 99, DayOfWeek = 4, SlotId = 3, RoomName = "C301" }
            };

            fakeRepo.SchedulesByClassIdResult = expected;

            var service = new ScheduleService(fakeRepo);

            var result = service.GetSchedulesByClassId(99);

            Assert.Same(expected, result);
            Assert.Equal(99, fakeRepo.LastClassIdForGetSchedulesByClassId);
        }

        [Fact]
        public void UpdateSchedule_ShouldCallRepository()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            var schedule = new Schedule
            {
                Id = 5,
                ClassId = 20,
                DayOfWeek = 5,
                SlotId = 4,
                RoomName = "D401"
            };

            service.UpdateSchedule(schedule);

            Assert.Equal(1, fakeRepo.UpdateScheduleCalledCount);
            Assert.Same(schedule, fakeRepo.LastUpdatedSchedule);
        }

        [Fact]
        public void DeleteSchedule_ShouldCallRepository()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            service.DeleteSchedule(8);

            Assert.Equal(1, fakeRepo.DeleteScheduleCalledCount);
            Assert.Equal(8, fakeRepo.LastDeletedScheduleId);
        }

        [Fact]
        public void DeleteByClassId_ShouldCallRepository()
        {
            var fakeRepo = new FakeScheduleRepository();
            var service = new ScheduleService(fakeRepo);

            service.DeleteByClassId(88);

            Assert.Equal(1, fakeRepo.DeleteByClassIdCalledCount);
            Assert.Equal(88, fakeRepo.LastDeletedClassId);
        }

        [Fact]
        public void GetAdminScheduleDetail_ShouldReturnRepositoryData()
        {
            var fakeRepo = new FakeScheduleRepository();
            var expected = new AdminScheduleDetailViewModel
            {
                ClassCode = "CL001",
                CourseCode = "C001",
                CourseName = "OOP"
            };

            fakeRepo.AdminDetailResult = expected;

            var service = new ScheduleService(fakeRepo);

            var result = service.GetAdminScheduleDetail(1, 2, 3);

            Assert.Same(expected, result);
        }

        [Fact]
        public void GetTeacherScheduleDetail_ShouldReturnRepositoryData()
        {
            var fakeRepo = new FakeScheduleRepository();
            var expected = new TeacherScheduleDetailViewModel
            {
                ClassCode = "CL002",
                CourseCode = "C002",
                CourseName = "PRN"
            };

            fakeRepo.TeacherDetailResult = expected;

            var service = new ScheduleService(fakeRepo);

            var result = service.GetTeacherScheduleDetail(9, 1, 2, 3);

            Assert.Same(expected, result);
        }

        [Fact]
        public void GetStudentScheduleDetail_ShouldReturnRepositoryData()
        {
            var fakeRepo = new FakeScheduleRepository();
            var expected = new StudentScheduleDetailViewModel
            {
                ClassCode = "CL003",
                CourseCode = "C003",
                CourseName = "DBI"
            };

            fakeRepo.StudentDetailResult = expected;

            var service = new ScheduleService(fakeRepo);

            var result = service.GetStudentScheduleDetail(11, 1, 2, 3);

            Assert.Same(expected, result);
        }

        private class FakeScheduleRepository : IScheduleRepository
        {
            public ScheduleWeekViewModel AdminWeeklyScheduleResult { get; set; } = new();
            public ScheduleWeekViewModel TeacherWeeklyScheduleResult { get; set; } = new();
            public ScheduleWeekViewModel StudentWeeklyScheduleResult { get; set; } = new();

            public ScheduleFilterOptionsViewModel AdminFilterOptionsResult { get; set; } = new();
            public ScheduleFilterOptionsViewModel TeacherFilterOptionsResult { get; set; } = new();

            public AdminScheduleDetailViewModel? AdminDetailResult { get; set; }
            public TeacherScheduleDetailViewModel? TeacherDetailResult { get; set; }
            public StudentScheduleDetailViewModel? StudentDetailResult { get; set; }

            public List<Schedule> AllSchedulesResult { get; set; } = new();
            public List<Schedule> SchedulesByClassIdResult { get; set; } = new();

            public int GetAdminWeeklyScheduleCalledCount { get; private set; }
            public int GetTeacherWeeklyScheduleCalledCount { get; private set; }
            public int GetStudentWeeklyScheduleCalledCount { get; private set; }

            public int GetAdminFilterOptionsCalledCount { get; private set; }
            public int GetTeacherFilterOptionsCalledCount { get; private set; }

            public int AddScheduleCalledCount { get; private set; }
            public int UpdateScheduleCalledCount { get; private set; }
            public int DeleteScheduleCalledCount { get; private set; }
            public int DeleteByClassIdCalledCount { get; private set; }

            public int? LastTeacherIdForWeeklySchedule { get; private set; }
            public int? LastStudentIdForWeeklySchedule { get; private set; }
            public int? LastTeacherIdForFilterOptions { get; private set; }
            public int? LastClassIdForGetSchedulesByClassId { get; private set; }
            public int? LastDeletedScheduleId { get; private set; }
            public int? LastDeletedClassId { get; private set; }

            public Schedule? LastAddedSchedule { get; private set; }
            public Schedule? LastUpdatedSchedule { get; private set; }

            public ScheduleWeekViewModel GetAdminWeeklySchedule(DateTime anyDateInWeek, ScheduleFilterViewModel? filter = null)
            {
                GetAdminWeeklyScheduleCalledCount++;
                return AdminWeeklyScheduleResult;
            }

            public ScheduleWeekViewModel GetTeacherWeeklySchedule(int teacherId, DateTime anyDateInWeek, ScheduleFilterViewModel? filter = null)
            {
                GetTeacherWeeklyScheduleCalledCount++;
                LastTeacherIdForWeeklySchedule = teacherId;
                return TeacherWeeklyScheduleResult;
            }

            public ScheduleWeekViewModel GetStudentWeeklySchedule(int studentId, DateTime anyDateInWeek)
            {
                GetStudentWeeklyScheduleCalledCount++;
                LastStudentIdForWeeklySchedule = studentId;
                return StudentWeeklyScheduleResult;
            }

            public ScheduleFilterOptionsViewModel GetAdminScheduleFilterOptions(DateTime anyDateInWeek)
            {
                GetAdminFilterOptionsCalledCount++;
                return AdminFilterOptionsResult;
            }

            public ScheduleFilterOptionsViewModel GetTeacherScheduleFilterOptions(int teacherId, DateTime anyDateInWeek)
            {
                GetTeacherFilterOptionsCalledCount++;
                LastTeacherIdForFilterOptions = teacherId;
                return TeacherFilterOptionsResult;
            }

            public AdminScheduleDetailViewModel? GetAdminScheduleDetail(int classId, int dayOfWeek, int slotId)
                => AdminDetailResult;

            public TeacherScheduleDetailViewModel? GetTeacherScheduleDetail(int teacherId, int classId, int dayOfWeek, int slotId)
                => TeacherDetailResult;

            public StudentScheduleDetailViewModel? GetStudentScheduleDetail(int studentId, int classId, int dayOfWeek, int slotId)
                => StudentDetailResult;

            public void AddSchedule(Schedule schedule)
            {
                AddScheduleCalledCount++;
                LastAddedSchedule = schedule;
            }

            public List<Schedule> GetAllSchedules()
                => AllSchedulesResult;

            public List<Schedule> GetSchedulesByClassId(int classId)
            {
                LastClassIdForGetSchedulesByClassId = classId;
                return SchedulesByClassIdResult;
            }

            public void UpdateSchedule(Schedule schedule)
            {
                UpdateScheduleCalledCount++;
                LastUpdatedSchedule = schedule;
            }

            public void DeleteSchedule(int id)
            {
                DeleteScheduleCalledCount++;
                LastDeletedScheduleId = id;
            }

            public void DeleteByClassId(int classId)
            {
                DeleteByClassIdCalledCount++;
                LastDeletedClassId = classId;
            }
        }
    }
}
