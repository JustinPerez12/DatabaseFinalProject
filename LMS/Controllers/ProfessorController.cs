using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var theStudents = (from classes in db.Classes
                               where classes.Season == season && classes.Year == year
                               join courses in db.Courses on classes.Listing equals courses.CatalogId
                               where courses.Number == num && courses.Department == subject
                               join enrolled in db.Enrolleds on classes.ClassId equals enrolled.Class
                               join students in db.Students on enrolled.Student equals students.UId
                               select new { fname = students.FName, lname = students.LName, uid = students.UId, dob = students.Dob, Grade = enrolled.Grade }
                            );

            if (theStudents == null || !theStudents.Any())
            {
                // no students
                return Json(null);
            }

            return Json(theStudents.ToArray());
        }

        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null)
            {
                // all assignments
                var theAssignments = (from classes in db.Classes
                                      where classes.Season == season && classes.Year == year
                                      join courses in db.Courses on classes.Listing equals courses.CatalogId
                                      where courses.Number == num && courses.Department == subject
                                      join categories in db.AssignmentCategories on classes.ClassId equals categories.InClass
                                      join assignments in db.Assignments on categories.CategoryId equals assignments.Category
                                      select new
                                      {
                                          cname = categories.Name,
                                          aname = assignments.Name,
                                          due = assignments.Due,
                                          submissions = db.Submissions.Count(s => s.Assignment == assignments.AssignmentId)
                                      }
                            );

                if (theAssignments == null | !theAssignments.Any())
                {
                    return Json(null);
                }

                return Json(theAssignments.ToArray());
            }
            else
            {
                // assignments from specified category
                var theAssignments = (from classes in db.Classes
                                      where classes.Season == season && classes.Year == year
                                      join courses in db.Courses on classes.Listing equals courses.CatalogId
                                      where courses.Number == num && courses.Department == subject
                                      join categories in db.AssignmentCategories on classes.ClassId equals categories.InClass
                                      where categories.Name == category
                                      join assignments in db.Assignments on categories.CategoryId equals assignments.Category
                                      select new
                                      {
                                          cname = categories.Name,
                                          aname = assignments.Name,
                                          due = assignments.Due,
                                          submissions = db.Submissions.Count(s => s.Assignment == assignments.AssignmentId)
                                      });

                if (theAssignments == null | !theAssignments.Any())
                {
                    return Json(null);
                }

                return Json(theAssignments.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var theCategories = (from classes in db.Classes
                                 where classes.Season == season && classes.Year == year
                                 join courses in db.Courses on classes.Listing equals courses.CatalogId
                                 where courses.Number == num && courses.Department == subject
                                 join categories in db.AssignmentCategories on classes.ClassId equals categories.InClass
                                 select new
                                 {
                                     name = categories.Name,
                                     weight = categories.Weight,
                                 }
                        );

            if (theCategories == null || !theCategories.Any())
            {
                return Json(null);
            }

            return Json(theCategories.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            // get class ID
            var classId = (from classes in db.Classes
                           where classes.Season == season && classes.Year == year
                           join courses in db.Courses on classes.Listing equals courses.CatalogId
                           where courses.Number == num && courses.Department == subject
                           select classes.ClassId
                        ).FirstOrDefault();

            var existingCategory =
                (from categories in db.AssignmentCategories
                where categories.Name == category && categories.InClass == classId
                select categories).FirstOrDefault();

            if (existingCategory != null)
            {
                // assignment category already exists
                return Json(new { success = false });
            }
            else
            {
                var newAssignmentCat = new AssignmentCategory();
                newAssignmentCat.Name = category;
                newAssignmentCat.Weight = (uint)catweight;
                newAssignmentCat.InClass = classId;

                db.AssignmentCategories.Add(newAssignmentCat);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            // get category ID
            var categoryId = (from classes in db.Classes
                              where classes.Season == season && classes.Year == year
                              join courses in db.Courses on classes.Listing equals courses.CatalogId
                              where courses.Number == num && courses.Department == subject
                              join categories in db.AssignmentCategories on classes.ClassId equals categories.InClass
                              where categories.Name == category
                              select categories.CategoryId
                        ).FirstOrDefault();

            var existingAssignment =
                (from assignments in db.Assignments
                where assignments.Name == asgname && assignments.Category == categoryId
                select assignments).FirstOrDefault();

            if (existingAssignment != null)
            {
                // assignment already exists
                return Json(new { success = false });
            }
            else
            {
                var newAssignment = new Assignment();
                newAssignment.Name = asgname;
                newAssignment.Contents = asgcontents;
                newAssignment.Due = asgdue;
                newAssignment.MaxPoints = (uint)asgpoints;
                newAssignment.Category = categoryId;

                db.Assignments.Add(newAssignment);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var theSubmissions = (from classes in db.Classes
                                  where classes.Season == season && classes.Year == year
                                  join courses in db.Courses on classes.Listing equals courses.CatalogId
                                  where courses.Number == num && courses.Department == subject
                                  join categories in db.AssignmentCategories on classes.ClassId equals categories.InClass
                                  where categories.Name == category
                                  join assignments in db.Assignments on categories.CategoryId equals assignments.Category
                                  where assignments.Name == asgname
                                  join submissions in db.Submissions on assignments.AssignmentId equals submissions.Assignment
                                  join students in db.Students on submissions.Student equals students.UId
                                  select new
                                  {
                                      fname = students.FName,
                                      lname = students.LName,
                                      uid = assignments.Due,
                                      time = submissions.Time,
                                      score = submissions.Score,
                                  }
                        );

            if (theSubmissions == null || !theSubmissions.Any())
            {
                return Json(null);
            }

            return Json(theSubmissions.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var professorClasses =
                (from classes in db.Classes
                 where classes.TaughtBy == uid
                 join course in db.Courses on classes.Listing equals course.CatalogId
                 select new { subject = course.Department, number = course.Number, name = course.Name, season = classes.Season, year = classes.Year }).Distinct();

            if (professorClasses == null || !professorClasses.Any())
            {
                // no classes
                return Json(null);
            }

            return Json(professorClasses.ToArray());
        }


        /*******End code to modify********/
    }
}

