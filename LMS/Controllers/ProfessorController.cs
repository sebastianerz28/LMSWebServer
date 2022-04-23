using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
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
            // Get's student's uid and grade based on parameters
            // select uID, Grade from Courses natural join Classes natural join Enrolled
            // where courseNum = <num> and Dept = "<subject>" and Season = "<season>" and Year = <year>;
            var studentsUidGrade = from course in db.Courses
                                   where course.CourseNum == num && course.Dept == subject
                                   join cl in db.Classes on course.CourseId equals cl.CourseId into leftJoinClasses
                                   from courseClass in leftJoinClasses
                                   where courseClass.Season == season && courseClass.Year == year
                                   join en in db.Enrolled on courseClass.ClassId equals en.ClassId
                                   select new
                                   {
                                       uid = en.UId,
                                       grade = en.Grade
                                   };
            var studentsInClass = from uidGrade in studentsUidGrade
                                  join s in db.Students
                                  on new { S = uidGrade.uid } equals new { S = s.UId }
                                  select new
                                  {
                                      fname = s.FName,
                                      lname = s.LName,
                                      uid = s.UId,
                                      dob = s.Dob,
                                      grade = uidGrade.grade
                                  };

            return Json(studentsInClass.ToArray());
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
            if (category is null)
            {
                var getclassIDCatID = from course in db.Courses
                                      where course.CourseNum == num && course.Dept == subject
                                      join classes in db.Classes on course.CourseId equals classes.CourseId into joinedCourseClass
                                      from courseClass in joinedCourseClass
                                      where courseClass.Season == season && courseClass.Year == year
                                      join assignCat in db.AssignmentCategories on courseClass.ClassId equals assignCat.ClassId
                                      select new
                                      {
                                          catName = assignCat.Name,
                                          catID = assignCat.CategoryId
                                      };
                // then join with Assignments to get necessary info
                var getAssigns = from catNameID in getclassIDCatID
                                 join a in db.Assignments
                                 on new { A = catNameID.catID } equals new { A = a.CategoryId }
                                 select new
                                 {
                                     aname = a.Name,
                                     cname = catNameID.catName,
                                     due = a.DueDate,
                                     submissions = a.Submissions.Count()
                                 };

                return Json(getAssigns.ToArray());
            }
            else
            {





                var query = from course in db.Courses
                            where course.CourseNum == num && course.Dept == subject
                            join classes in db.Classes on course.CourseId equals classes.CourseId into joinedCourseClass
                            from courseClass in joinedCourseClass
                            where courseClass.Season == season && courseClass.Year == year
                            join assignCat in db.AssignmentCategories on courseClass.ClassId equals assignCat.ClassId
                            where assignCat.Name == category
                            join assignments in db.Assignments on assignCat.CategoryId equals assignments.CategoryId
                            select new
                            {
                                aname = assignments.Name,
                                catID = assignCat.CategoryId,
                                due = assignments.DueDate,
                                submissions = assignments.Submissions.Count()
                            };
                return Json(query.ToArray());
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
            var catNameWeight = from course in db.Courses
                                where course.CourseNum == num && course.Dept == subject
                                join classes in db.Classes on course.CourseId equals classes.CourseId into joinedCourseClass
                                from courseClass in joinedCourseClass
                                where courseClass.Season == season && courseClass.Year == year
                                join assignCat in db.AssignmentCategories on courseClass.ClassId equals assignCat.ClassId
                                select new
                                {
                                    name = assignCat.Name,
                                    weight = assignCat.Weight
                                };

            return Json(catNameWeight.ToArray());
        }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false},
    ///	false if an assignment category with the same name already exists in the same class.</returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
    {
            var findClassID = from course in db.Courses
                              where course.CourseNum == num && course.Dept == subject
                              join classes in db.Classes on course.CourseId equals classes.CourseId into joinedCourseClass
                              from courseClass in joinedCourseClass
                              where courseClass.Season == season && courseClass.Year == year
                              select new
                              {
                                  classID = courseClass.ClassId
                              };

            try
            {
                AssignmentCategories a = new AssignmentCategories()
                {
                    Name = category,
                    Weight = (uint)catweight,
                    ClassId = findClassID.First().classID,
                };
                db.AssignmentCategories.Add(a);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
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
    /// <returns>A JSON object containing success = true/false,
	/// false if an assignment with the same name already exists in the same assignment category.</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
            var asgnmtExists = from courses in db.Courses
                               where courses.Dept == subject && courses.CourseNum == num
                               join classes in db.Classes on courses.CourseId equals classes.CourseId into joinedCourseClass
                               from courseClass in joinedCourseClass
                               where courseClass.Season == season && courseClass.Year == year
                               join assignmentCat in db.AssignmentCategories on courseClass.ClassId equals assignmentCat.ClassId into joinedClassesCat
                               from classesCat in joinedClassesCat
                               where classesCat.Name == category
                               select new
                               {
                                   courseClass.ClassId,
                                   asgnCatID = classesCat.CategoryId,
                                   asgnCatName = classesCat.Name
                               };

            try
            {
                Assignments a = new Assignments()
                {
                    Name = asgname,
                    Contents = asgcontents,
                    DueDate = asgdue,
                    Points = (uint)asgpoints,
                    CategoryId = asgnmtExists.First().asgnCatID
                };

                

                db.Assignments.Add(a);
                db.SaveChanges();

                var findEnrolled = from enrolled in db.Enrolled
                                   where enrolled.ClassId == asgnmtExists.First().ClassId
                                   select new { enrolled.UId };
                foreach(var v in findEnrolled.ToArray())
                {
                    string newLetterGrade = CalulateGrade(asgnmtExists.First().ClassId, v.UId);
                    Enrolled e = new Enrolled();
                    e.ClassId = asgnmtExists.First().ClassId;
                    e.Grade = newLetterGrade;
                    e.UId = v.UId;
                    db.Enrolled.Update(e);
                    db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
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
            var query = from courses in db.Courses
                        where courses.Dept == subject && courses.CourseNum == num
                        join classes in db.Classes on courses.CourseId equals classes.CourseId into joinedCourseClass
                        from courseClass in joinedCourseClass
                        where courseClass.Year == year && courseClass.Season == season
                        join categories in db.AssignmentCategories on courseClass.ClassId equals categories.ClassId into joinedclassCat
                        from classCat in joinedclassCat
                        where classCat.Name == category
                        join assignments in db.Assignments on classCat.CategoryId equals assignments.CategoryId into joinedCatAssign
                        from catAssign in joinedCatAssign
                        where catAssign.Name == asgname
                        join submissions in db.Submissions on catAssign.AssignmentId equals submissions.AssignmentId into joinedSubAssign
                        from subAssign in joinedSubAssign
                        select new
                        {
                            subUID = subAssign.UId,
                            subTime = subAssign.Time,
                            subScore = subAssign.Score
                        };
            var query2 = from q in query
                         join s in db.Students
                         on new { A = q.subUID } equals new { A = s.UId }
                         into joined
                         from j in joined.DefaultIfEmpty()
                         select new
                         {
                             fname = j.FName,
                             lname = j.LName,
                             uid = j.UId,
                             time = q.subTime,
                             score = q.subScore
                         };
            return Json(query2.ToArray());
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
            var query = from courses in db.Courses
                        where courses.Dept == subject && courses.CourseNum == num
                        join classes in db.Classes on courses.CourseId equals classes.CourseId into joinedCourseClass
                        from courseClass in joinedCourseClass
                        where courseClass.Year == year && courseClass.Season == season
                        join categories in db.AssignmentCategories on courseClass.ClassId equals categories.ClassId into joinedclassCat
                        from classCat in joinedclassCat
                        where classCat.Name == category
                        join assignments in db.Assignments on classCat.CategoryId equals assignments.CategoryId into joinedCatAssign
                        from catAssign in joinedCatAssign
                        where catAssign.Name == asgname
                        join submissions in db.Submissions on catAssign.AssignmentId equals submissions.AssignmentId into joinedSubAssign
                        from subAssign in joinedSubAssign
                        where subAssign.UId == uid
                        select new
                        {
                            aID = subAssign.AssignmentId,
                            uID = subAssign.UId,
                            contents = subAssign.Contents,
                            time = subAssign.Time,
                            classID = classCat.ClassId                           
                        };
            try
            {
                
                Submissions s = new Submissions
                {
                    UId = uid,
                    Time = query.First().time,
                    Score = (uint?)score,
                    Contents = query.First().contents,
                    AssignmentId = query.First().aID
                };

                db.Submissions.Update(s);
                db.SaveChanges();
                string letterGrade = CalulateGrade(query.First().classID, uid);
                Enrolled e = new Enrolled();
                e.ClassId = query.First().classID;
                e.Grade = letterGrade;
                e.UId = uid;
                db.Enrolled.Update(e);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false });
            }
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
            var query = from classes in db.Classes
                        where classes.InstructorId == uid
                        select new
                        {
                            subject = classes.Course.Dept,
                            number = classes.Course.CourseNum,
                            name = classes.Course.Name,
                            season = classes.Season,
                            year = classes.Year
                        };
      return Json(query.ToArray());
    }
    

    public string CalulateGrade(uint classID, string uid)
    {
            //Need Assignments
            //Assignment Category weight
            //Number of Assignments

            var assignCats = from assignmentCategories in db.AssignmentCategories
                             where assignmentCategories.ClassId == classID
                             select new
                             {
                                 name = assignmentCategories.Name,
                                 weight = assignmentCategories.Weight,
                                 assignments = assignmentCategories.Assignments,
                                 catID = assignmentCategories.CategoryId
                             };
            List<double?> percentages = new List<double?>();
            double catWeights = 0;
            foreach(var v in assignCats)
            {
                
                if(v.assignments.Count() > 0)
                {
                    var totalEarnedPoints = from submissions in db.Submissions
                                            where submissions.UId == uid && submissions.Assignment.Category.CategoryId == v.catID
                                            select new
                                            {
                                                submissions.Score
                                            };
                    uint? earnedPoints = 0;
                    foreach (var x in totalEarnedPoints)
                    {
                        if(x.Score != null)
                            earnedPoints += x.Score;
                    }
                    uint pointsCat = 0;
                    foreach (var g in v.assignments.ToArray())
                    {
                        pointsCat += g.Points;
                    }
                    double? scoreForCat = ((double?)earnedPoints / pointsCat) * v.weight;
                    percentages.Add(scoreForCat);
                    catWeights += v.weight;

                }
                
            }
            double? percentageSum = 0;
            foreach (double? percentage in percentages)
                percentageSum += percentage;
            double? scaleFactor = 100 / catWeights;
            double? finalScore = scaleFactor * percentageSum;

            if (finalScore > 93 && finalScore <= 100)
                return "A";
            else if (finalScore >= 90 && finalScore < 93)
                return "A-";
            else if (finalScore >= 87 && finalScore < 90)
                return "B+";
            else if (finalScore >= 83 && finalScore < 87)
                return "B";
            else if (finalScore >= 80 && finalScore < 83)
                return "B-";
            else if (finalScore >= 77 && finalScore < 80)
                return "C+";
            else if (finalScore >= 73 && finalScore < 77)
                return "C";
            else if (finalScore >= 70 && finalScore < 73)
                return "C-";
            else if (finalScore >= 67 && finalScore < 70)
                return "D+";
            else if (finalScore >= 63 && finalScore < 67)
                return "D";
            else if (finalScore >= 60 && finalScore < 67)
                return "D-";
            else
                return "E";

        }

    /*******End code to modify********/

  }
}