using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Courses
    {
        public Courses()
        {
            Classes = new HashSet<Classes>();
        }

        public uint CourseId { get; set; }
        public string Name { get; set; }
        public uint CourseNum { get; set; }
        public string Dept { get; set; }

        public virtual Departments DeptNavigation { get; set; }
        public virtual ICollection<Classes> Classes { get; set; }
    }
}
