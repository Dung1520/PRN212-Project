using BusinessObjects;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject1
{
    public class CourseServiceTests
    {
        [Fact]
        public void AddCourse_ValidData_ShouldSuccess()
        {
            var repo = new FakeCourseRepository();
            var service = new CourseService(repo);

            var course = new Course
            {
                CourseCode = "C001",
                Name = "Java Basic",
                DurationWeeks = 4,
                Fee = 100
            };

            service.AddCourse(course);

            Assert.Single(repo.courses);
            Assert.Equal("Open", course.Status); // auto set
        }
    }
}
