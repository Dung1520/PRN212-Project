using Xunit;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using DataAccess;
using Services;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject1
{
    public class ClassServiceTests
    {
        private LctmsDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<LctmsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)
                )
                .Options;

            return new LctmsDbContext(options);
        }

        private void SeedData(LctmsDbContext context)
        {
            context.Courses.Add(new Course
            {
                Id = 1,
                CourseCode = "C001",
                Name = "Test Course",
                DurationWeeks = 4,
                Fee = 100,
                Status = "Open"
            });

            context.Teachers.Add(new Teacher
            {
                Id = 1,
                Username = "teacher1",
                TeacherCode = "T001",
                Email = "t@test.com",
                Password = "123",
                FullName = "Teacher 1"
            });

            context.Slots.Add(new Slot
            {
                Id = 1,
                SlotName = "Slot 1",
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10)
            });

            context.SaveChanges();
        }

        // ✅ 1. SUCCESS CASE
        [Fact]
        public void AddClass_ValidData_ShouldSuccess()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new ClassRepository(context);
            var service = new ClassService(repo, context);

            var newClass = new Class
            {
                ClassCode = "CL001",
                Capacity = 30,
                CourseId = 1,
                TeacherId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            };

            var schedules = new List<Schedule>
            {
                new Schedule
                {
                    DayOfWeek = 1,
                    SlotId = 1,
                    RoomName = "A101"
                }
            };

            service.AddClass(newClass, schedules);

            Assert.Equal(1, context.Classes.Count());
            Assert.Equal(1, context.Schedules.Count());
        }

        // ❌ 2. TRÙNG SLOT (Duplicate schedule)
        [Fact]
        public void AddClass_DuplicateSchedule_ShouldThrowException()
        {
            var context = GetDbContext();
            SeedData(context);

            var repo = new ClassRepository(context);
            var service = new ClassService(repo, context);

            var newClass = new Class
            {
                ClassCode = "CL002",
                Capacity = 30,
                CourseId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            };

            var schedules = new List<Schedule>
            {
                new Schedule { DayOfWeek = 1, SlotId = 1, RoomName = "A101" },
                new Schedule { DayOfWeek = 1, SlotId = 1, RoomName = "B101" } // ❌ trùng
            };

            Assert.Throws<Exception>(() => service.AddClass(newClass, schedules));
        }

        // ❌ 3. TRÙNG PHÒNG
        [Fact]
        public void AddClass_RoomConflict_ShouldThrowException()
        {
            var context = GetDbContext();
            SeedData(context);

            // seed class cũ
            context.Classes.Add(new Class
            {
                Id = 1,
                ClassCode = "OLD",
                Capacity = 20,
                CourseId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            });

            context.Schedules.Add(new Schedule
            {
                ClassId = 1,
                DayOfWeek = 1,
                SlotId = 1,
                RoomName = "A101"
            });

            context.SaveChanges();

            var repo = new ClassRepository(context);
            var service = new ClassService(repo, context);

            var newClass = new Class
            {
                ClassCode = "CL003",
                Capacity = 30,
                CourseId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            };

            var schedules = new List<Schedule>
            {
                new Schedule { DayOfWeek = 1, SlotId = 1, RoomName = "A101" } // ❌ trùng phòng
            };

            Assert.Throws<Exception>(() => service.AddClass(newClass, schedules));
        }

        // ❌ 4. TRÙNG GIÁO VIÊN
        [Fact]
        public void AddClass_TeacherConflict_ShouldThrowException()
        {
            var context = GetDbContext();
            SeedData(context);

            context.Classes.Add(new Class
            {
                Id = 1,
                ClassCode = "OLD",
                TeacherId = 1,
                Capacity = 20,
                CourseId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            });

            context.Schedules.Add(new Schedule
            {
                ClassId = 1,
                DayOfWeek = 1,
                SlotId = 1,
                RoomName = "A101"
            });

            context.SaveChanges();

            var repo = new ClassRepository(context);
            var service = new ClassService(repo, context);

            var newClass = new Class
            {
                ClassCode = "CL004",
                Capacity = 30,
                CourseId = 1,
                TeacherId = 1, // ❌ cùng teacher
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(10),
                Status = "Open"
            };

            var schedules = new List<Schedule>
            {
                new Schedule { DayOfWeek = 1, SlotId = 1, RoomName = "B101" }
            };

            Assert.Throws<Exception>(() => service.AddClass(newClass, schedules));
        }
    }
}