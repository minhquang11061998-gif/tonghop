using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataBase.Models
{
    public class Subject_Grade
    {
        [Key]
        public Guid Id { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid GradeId { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        public virtual Grades? Grade { get; set; }
        public virtual Subjects? Subject { get; set; }
    }
}
