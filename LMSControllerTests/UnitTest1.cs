using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDepartmentDB());

            var allDepts = ctrl.GetDepartments() as JsonResult;

            dynamic x = allDepts.Value;

            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        [Fact]
        public void TestGetGPA()
        {
            // An example of a simple unit test on the CommonController
            StudentController ctrl = new StudentController(MakeTinyGPADB());

            var result = ctrl.GetGPA("testUID") as JsonResult;

            dynamic gpa = result.Value;

            Assert.Equal(4.0, gpa);
        }



        /// <summary>
        /// Make a very tiny in-memory database, containing just one department
        /// and nothing else.
        /// </summary>
        /// <returns></returns>
        LMSContext MakeTinyDepartmentDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }


        /// <summary>
        /// Make a very tiny in-memory database, containing just one department
        /// and nothing else.
        /// </summary>
        /// <returns></returns>
        LMSContext MakeTinyGPADB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // add classes
            db.Classes.Add(new Class {
                ClassId = 1234,
                Season = "Spring",
                Year = 2023,
                Location = "Building A, Room 101",
                StartTime = new TimeOnly(9, 0, 0),
                EndTime = new TimeOnly(10, 30, 0),
                Listing = 5678,
                TaughtBy = "Professor Smith"
            });
            db.Classes.Add(new Class
            {
                ClassId = 4321,
                Season = "Fall",
                Year = 2023,
                Location = "Building B, Room 202",
                StartTime = new TimeOnly(13, 0, 0),
                EndTime = new TimeOnly(14, 30, 0),
                Listing = 8765,
                TaughtBy = "Professor Johnson"
            });

            // add student
            db.Students.Add(new Student
            {
                UId = "testUID",
                FName = "John",
                LName = "Doe",
                Dob = new DateOnly(2001, 5, 1),
                Major = "Computer Science"
            });

            //add enrolled
            db.Enrolleds.Add(new Enrolled
            {
                Student = "testUID",
                Class = 1234,
                Grade = "A",
            });
            db.Enrolleds.Add(new Enrolled
            {
                Student = "testUID",
                Class = 4321,
                Grade = "--",
            });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}