using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignments
    {
        public Assignments()
        {
            Submissions = new HashSet<Submissions>();
        }

        public string Name { get; set; }
        public string Contents { get; set; }
        public DateTime DueDate { get; set; }
        public uint Points { get; set; }
        public uint CategoryId { get; set; }
        public uint AssignmentId { get; set; }

        public virtual AssignmentCategories Category { get; set; }
        public virtual ICollection<Submissions> Submissions { get; set; }
    }
}
