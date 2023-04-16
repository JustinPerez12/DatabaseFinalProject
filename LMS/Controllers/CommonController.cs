using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var departments =
                from department in db.Departments
                select new { name = department.Name, subject = department.Subject };

            return Json(departments.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var courseCatalog =
                from department in db.Departments
                join courses in (
                    from c in db.Courses
                    group c by c.Department into courseGroup
                    select new { Department = courseGroup.Key, Courses = courseGroup.Select(c => new { number = c.Number, cname = c.Name }).ToArray() }
                ) on department.Subject equals courses.Department
                select new
                {
                    subject = department.Subject,
                    dname = department.Name,
                    courses = courses.Courses
                };


            return Json(courseCatalog.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var classOfferings =
                from course in db.Courses
                where course.Department == subject && course.Number == number
                join classes in db.Classes on course.CatalogId equals classes.Listing into allOfferings
                from z in allOfferings
                join professor in db.Professors on z.TaughtBy equals professor.UId into professor
                from x in professor.DefaultIfEmpty()
                select new { season = z.Season, year = z.Year, location = z.Location, start = z.StartTime, end = z.EndTime, fname = x.FName == null ? "" : x.FName, lname = x.LName == null ? "" : x.LName };
      

            return Json(classOfferings.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var getAssignment =
                (from course in db.Courses
                where course.Department == subject && course.Number == num
                join classes in db.Classes on course.CatalogId equals classes.Listing
                from assignCats in db.AssignmentCategories
                where classes.ClassId == assignCats.InClass
                join assignment in db.Assignments on assignCats.CategoryId equals assignment.Category
                where assignment.Name == asgname
                select assignment.Contents).First();
                
               
            return Content(getAssignment);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var getSubmission =
                (from course in db.Courses
                 where course.Department == subject && course.Number == num
                 join classes in db.Classes on course.CatalogId equals classes.Listing
                 from assignCats in db.AssignmentCategories
                 where classes.ClassId == assignCats.InClass
                 join assignment in db.Assignments on assignCats.CategoryId equals assignment.Category
                 where assignment.Name == asgname
                 join submission in db.Submissions on assignment.AssignmentId equals submission.Assignment
                 where submission.Student == uid
                 select submission.SubmissionContents).FirstOrDefault();

            if(getSubmission == null)
            {
                return Content("");
            }
            else
            {
                return Content(getSubmission);
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            //check if they are a professor
            var professor =
                (from professors in db.Professors
                where professors.UId == uid
                join department in db.Departments on professors.WorksIn equals department.Subject
                select new { fname = professors.FName, lname = professors.LName, uid = professors.UId, department = professors.WorksIn }).FirstOrDefault();

            if (professor != null)
            {
                return Json(professor);
            }

            //check if the are a student
            var student =
                (from students in db.Students
                where students.UId == uid
                join department in db.Departments on students.Major equals department.Subject
                select new { fname = students.FName, lname = students.LName, uid = students.UId, department = department.Name }).FirstOrDefault();

            if (student != null)
            {
                return Json(student);
            }

            //check if they are an admin
            var admin =
                (from admins in db.Administrators
                where admins.UId == uid
                select new { fname = admins.FName, lname = admins.LName, uid = admins.UId }).FirstOrDefault();            
            
            if (admin != null) 
            {
                return Json(admin);
            }
            else 
            { 
                return Json(new { success = false });
            }
        }
        /*******End code to modify********/
    }
}

