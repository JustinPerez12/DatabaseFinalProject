using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            var departments =
                from department in db.Departments
                select department.Subject;

            if (departments.Contains(subject))
            {
                return Json(new { success = false });
            }
            else
            {
                var newDepartment = new Department();
                newDepartment.Subject = subject;
                newDepartment.Name = name;
                db.Departments.Add(newDepartment);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var allCourses =
                from courses in db.Courses
                where courses.Department == subject
                select new { number = courses.Number, name = courses.Name };

            return Json(allCourses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var professorsInSubject =
                from professors in db.Professors
                where professors.WorksIn == subject
                select new { lname = professors.LName, fname = professors.FName, uid = professors.UId };

            return Json(professorsInSubject.ToArray());

        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var allCourses =
                from course in db.Courses
                select course;
            bool exists = allCourses.Any(course => course.Department == subject && course.Number == number);

            if (exists)
            {
                return Json(new { success = false });
            }
            else
            {
                var newCourse = new Course();
                newCourse.Number = (uint)number;
                newCourse.Name = name;
                newCourse.Department = subject;
                db.Courses.Add(newCourse);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var allClasses =
                 from classes in db.Classes where classes.Season == season && classes.Year == year
                 select classes;


            //check if it occupoes the same location during the same time as another class
            if (allClasses != null)
            {
                foreach (Class tempClass in allClasses)
                {
                    if (tempClass.Location == location)
                    {
                        if(tempClass.StartTime <= TimeOnly.FromDateTime(start) && TimeOnly.FromDateTime(start) <= tempClass.EndTime)
                        {
                            return Json(new { success = false });
                        }
                        else if(tempClass.StartTime <= TimeOnly.FromDateTime(end) && TimeOnly.FromDateTime(end) <= tempClass.EndTime)
                        {
                            return Json(new { success = false });
                        }
                    }
                }
            }
            //class can be added because there arent any classes at all for this season and year
            else
            {
                var currCourse =
                 (from courses in db.Courses
                  where courses.Department == subject && courses.Number == number
                  select courses).First();

                var newClass = new Class();
                newClass.Year = (uint)year;
                newClass.Season = season;
                newClass.Listing = currCourse.CatalogId;
                newClass.TaughtBy = instructor;
                newClass.Location = location;
                newClass.StartTime = TimeOnly.FromDateTime(start);
                newClass.EndTime = TimeOnly.FromDateTime(end);
                db.Classes.Add(newClass);
                db.SaveChanges();
            }

            //query to see if class exists
            var currClass =
                (from courses in db.Courses where courses.Department == subject && courses.Number == number
                join classes in db.Classes on season equals classes.Season where classes.Year == year && courses.CatalogId == classes.Listing
                select classes).FirstOrDefault();

            // class exists dont make it
            if(currClass != null )
            {
                return Json(new { success = false });
            }
            else
            {
                var currCourse =
                 (from courses in db.Courses
                 where courses.Department == subject && courses.Number == number
                 select courses).First();

                var newClass = new Class();
                newClass.Year = (uint)year;
                newClass.Season = season;
                newClass.Listing = currCourse.CatalogId;
                newClass.TaughtBy = instructor;
                newClass.Location = location;
                newClass.StartTime = TimeOnly.FromDateTime(start);
                newClass.EndTime = TimeOnly.FromDateTime(end);
                db.Classes.Add(newClass);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }


        /*******End code to modify********/

    }
}

