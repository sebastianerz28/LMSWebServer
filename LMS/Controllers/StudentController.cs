using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models;
using LMS.Models.AccountViewModels;
using LMS.Services;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
  [Authorize(Roles = "Student")]
  public class StudentController : CommonController
  {

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
            // select Dept, courseNum, Courses.Name, Season, Year, Grade from
            // Students natural join Enrolled natural join Classes natural join Courses
            // where Students.uID="<uid>";
            var query = from s in db.Students
                        where s.UId == uid
                        join e in db.Enrolled on s.UId equals e.UId into leftJoinEnrolled
                        from x in leftJoinEnrolled.DefaultIfEmpty()
                        join cl in db.Classes on x.ClassId equals cl.ClassId into leftJoinClasses
                        from y in leftJoinClasses.DefaultIfEmpty()
                        join co in db.Courses on y.CourseId equals co.CourseId into leftJoinCourses
                        from z in leftJoinCourses.DefaultIfEmpty()
                        select new
                        {
                            subject = z.Dept,
                            number = z.CourseNum,
                            name = z.Name,
                            season = y.Season,
                            year = y.Year,
                            grade = x.Grade == null ? "--" : x.Grade
                        };

            return Json(query.ToArray());
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
      // select Assignments.Name as aname, AssignmentCategories.Name as cname, Assignments.DueDate as due, Submissions.Score
      // from Assignments join AssignmentCategories join Submissions
      // where Assignments.CategoryID=AssignmentCategories.CategoryID
      // and Submissions.AssignmentID=Assignments.AssignmentID
      // and ClassID=
      //    (select ClassID from Enrolled natural join Classes natural join Courses
      //        where uID="<uid>" and Year=<year> and Season ="<season>" and courseNum=<num>);

        var query = from enrolled in db.Enrolled
                    where enrolled.UId == uid
                    from courses in db.Courses
                    where courses.Dept == subject && courses.CourseNum == num
                    join classes in db.Classes on courses.CourseId equals classes.CourseId into joinedCourseClass
                    from courseClass in joinedCourseClass
                    where courseClass.Season == season && courseClass.Year == year && enrolled.ClassId == courseClass.ClassId
                    join assignmentCat in db.AssignmentCategories on courseClass.ClassId equals assignmentCat.ClassId into joinedClassesCat
                    from classesCat in joinedClassesCat
                    join assignments in db.Assignments on classesCat.CategoryId equals assignments.CategoryId
                    select new
                    {
                        aname = assignments.Name,
                        cname = classesCat.Name,
                        due = assignments.DueDate,
                        assignmentID = assignments.AssignmentId
                    };
        var query2 = from q in query
                     join s in db.Submissions
                     on new { A = q.assignmentID, B = uid } equals new { A = s.AssignmentId, B = s.UId }
                     into joined
                     from j in joined.DefaultIfEmpty()
                     select new
                     {
                         aname = q.aname,
                         cname = q.cname,
                         due = q.due,
                         score = j.Score == null ? null : (uint?)j.Score
                     };
            return Json(query2);
    }



    /// <summary>
    /// Adds a submission to the given assignment for the given student
    /// The submission should use the current time as its DateTime
    /// You can get the current time with DateTime.Now
    /// The score of the submission should start as 0 until a Professor grades it
    /// If a Student submits to an assignment again, it should replace the submission contents
    /// and the submission time (the score should remain the same).
	/// Does *not* automatically reject late submissions.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="uid">The student submitting the assignment</param>
    /// <param name="contents">The text contents of the student's submission</param>
    /// <returns>A JSON object containing {success = true/false}.</returns>
    public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, 
      string category, string asgname, string uid, string contents)
    {
            // to get assignmentID
            // select AssignmentID from
            //  (select CategoryID from Courses join Classes join AssignmentCategories
            //      where Courses.courseID = Classes.courseID
            //      and Classes.ClassID = AssignmentCategories.ClassID
            //      and courseNum = <num>
            //      and Dept = "<subject>")
            //  as c natural join Assignments;
            var query1 = from course in db.Courses
                         where course.CourseNum == num && course.Dept == subject
                         join classes in db.Classes on course.CourseId equals classes.CourseId into joinedCourseClass
                         from courseClass in joinedCourseClass
                         join assignCat in db.AssignmentCategories on courseClass.ClassId equals assignCat.ClassId
                         select new
                         {
                             catID = assignCat.CategoryId
                         };
            var query2 = from q in query1
                         join a in db.Assignments 
                         on new { A = q.catID } equals new { A = a.CategoryId }
                         select new
                         {
                             asgnID = a.AssignmentId
                         };
            var aID = query2.First().asgnID;
            Submissions sub = new Submissions
            {
                Time = DateTime.Now,
                Contents = contents,
                Score = 0,
                UId = uid,
                AssignmentId = aID
            };
            db.Submissions.Add(sub);
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
    /// <returns>A JSON object containing {success = {true/false},
	/// false if the student is already enrolled in the Class.</returns>
    public IActionResult Enroll(string subject, int num, string season, int year, string uid)
    {
            // insert into Enrolled values("--", "<uid>", (select classID from Courses natural join Classes
            //                                              where Dept="subject"
            //                                              and courseNum=<num>
            //                                              and Season="<season>"
            //                                              and Year="<year>"));

            var query1 = from course in db.Courses
                         where course.Dept == subject && course.CourseNum == num
                         join cl in db.Classes on course.CourseId equals cl.CourseId into leftJoinClasses
                         from courseClass in leftJoinClasses
                         where courseClass.Season == season && courseClass.Year == year
                         select new
                         {
                             classID = courseClass.ClassId
                         };
            var cID = query1.First().classID;
            var query2 = from e in db.Enrolled
                         where e.UId == uid && cID == e.ClassId
                         select new
                         {
                             classID = e.ClassId
                         };
            if (query2.Any())   // if any classIDs are returned, student is enrolled, return false
            {
                return Json(new { success = false });
            }
            else
            {
                // perform insert
                Enrolled e = new Enrolled
                {
                    Grade = "--",
                    UId = uid,
                    ClassId = cID
                };
                db.Enrolled.Add(e);
                db.SaveChanges();

                return Json(new { success = true });
            }
    }



    /// <summary>
    /// Calculates a student's GPA
    /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
    /// Assume all classes are 4 credit hours.
    /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
    /// If a student does not have any grades, they have a GPA of 0.0.
    /// Otherwise, the point-value of a letter grade is determined by the table on this page:
    /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
    public IActionResult GetGPA(string uid)
    {
            var allGrades = from grade in db.Enrolled
                            where uid == grade.UId
                            select new
                            {
                                grades = grade.Grade
                            };
            int classesWithGrades = allGrades.Count();
            Double gradePoints = 0.0;
            foreach (var g in allGrades)
            {
                switch (g.grades)
                {
                    case "A":
                        gradePoints += 4.0;
                        break;
                    case "A-":
                        gradePoints += 3.7;
                        break;
                    case "B+":
                        gradePoints += 3.3;
                        break;
                    case "B":
                        gradePoints += 3.0;
                        break;
                    case "B-":
                        gradePoints += 2.7;
                        break;
                    case "C+":
                        gradePoints += 2.3;
                        break;
                    case "C":
                        gradePoints += 2.0;
                        break;
                    case "C-":
                        gradePoints += 1.7;
                        break;
                    case "D+":
                        gradePoints += 1.3;
                        break;
                    case "D-":
                        gradePoints += 0.7;
                        break;
                    case "E":
                        break;
                    case "--":
                        classesWithGrades--;
                        break;
                }
            }
            Double gpa = 0.0;
            if (classesWithGrades > 0)
            {
                gpa = gradePoints / classesWithGrades;
            }
            IDictionary<string, Double> gpa2 = new Dictionary<string, Double>();
            gpa2.Add("gpa", gpa); //adding a key/value using the Add() method
            
            return Json(gpa2);
    }

    /*******End code to modify********/

  }
}