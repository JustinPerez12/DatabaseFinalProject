using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var studentsClasses =
                from enroll in db.Enrolleds
                join student in db.Students on enroll.Student equals uid
                join classes in db.Classes on enroll.Class equals classes.ClassId
                join courses in db.Courses on classes.Listing equals courses.CatalogId
                select new {subject = courses.Department, number = courses.Number, name = courses.Name, season = classes.Season, year = classes.Year, grade = enroll.Grade == null ? "--" : enroll.Grade };

            return Json(studentsClasses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var classAssignments =
                from courses in db.Courses
                where courses.Department == subject && courses.Number == num
                join classes in db.Classes on courses.CatalogId equals classes.Listing
                where classes.Season == season && classes.Year == year
                from assignCat in db.AssignmentCategories
                where classes.ClassId == assignCat.InClass
                join assignments in db.Assignments on assignCat.CategoryId equals assignments.Category
                from submission in db.Submissions.Where(sub => sub.Assignment == assignments.AssignmentId && sub.Student == uid).DefaultIfEmpty()
                select new { aname = assignments.Name, cname = assignCat.Name, due = assignments.Due, score = submission.Score == null ? null : submission.Score };

            return Json(classAssignments.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var asgSubmission =
               (from courses in db.Courses
               where courses.Department == subject && courses.Number == num
               join classes in db.Classes on courses.CatalogId equals classes.Listing
               where classes.Season == season && classes.Year == year
               from assignCat in db.AssignmentCategories
               where classes.ClassId == assignCat.InClass && assignCat.Name == category
               join assignments in db.Assignments on asgname equals assignments.Name
               from submission in db.Submissions.Where(sub => sub.Assignment == assignments.AssignmentId && sub.Student == uid).DefaultIfEmpty()
               select submission).First();

            if(asgSubmission == null)
            {
                var assignment =
                (from courses in db.Courses
                where courses.Department == subject && courses.Number == num
                join classes in db.Classes on courses.CatalogId equals classes.Listing
                where classes.Season == season && classes.Year == year
                from assignCat in db.AssignmentCategories
                where classes.ClassId == assignCat.InClass && assignCat.Name == category
                join assignments in db.Assignments on asgname equals assignments.Name
                select assignments).First();

                var newSubmission = new Submission();
                newSubmission.SubmissionContents = contents;
                newSubmission.Student = uid;
                newSubmission.Score = 0;
                newSubmission.Time = DateTime.Now;
                newSubmission.Assignment = assignment.AssignmentId;
                db.Submissions.Add(newSubmission);
                db.SaveChanges();
                return Json(new { success = true });
            }

            asgSubmission.SubmissionContents = contents;
            asgSubmission.Time = DateTime.Now;
            db.Submissions.Update(asgSubmission);
            db.SaveChanges();
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var isEnrolled =
              (from courses in db.Courses
               where courses.Department == subject && courses.Number == num
               join classes in db.Classes on courses.CatalogId equals classes.Listing
               where classes.Season == season && classes.Year == year
               join enrolled in db.Enrolleds on classes.ClassId equals enrolled.Class
               select enrolled).FirstOrDefault();

            if(isEnrolled != null)
            {
                return Json(new { success = false });
            }

            var classToEnroll =
             (from courses in db.Courses
              where courses.Department == subject && courses.Number == num
              join classes in db.Classes on courses.CatalogId equals classes.Listing
              where classes.Season == season && classes.Year == year
              select classes).First();

            var newStudent = new Enrolled();
            newStudent.Student = uid;
            newStudent.Class = classToEnroll.ClassId;
            newStudent.Grade = "--";
            classToEnroll.Enrolleds.Add(newStudent);
            db.Classes.Update(classToEnroll);
            db.SaveChanges();

            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {    

            return Json(null);
        }
                
        /*******End code to modify********/

    }
}

