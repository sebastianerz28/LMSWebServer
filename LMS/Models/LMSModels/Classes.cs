using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Classes
    {
        public Classes()
        {
            AssignmentCategories = new HashSet<AssignmentCategories>();
            Enrolled = new HashSet<Enrolled>();
        }

        public string Loc { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public uint Year { get; set; }
        public string Season { get; set; }
        public uint CourseId { get; set; }
        public string InstructorId { get; set; }
        public uint ClassId { get; set; }

        public virtual Courses Course { get; set; }
        public virtual Professors Instructor { get; set; }
        public virtual ICollection<AssignmentCategories> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolled { get; set; }
    }
}
