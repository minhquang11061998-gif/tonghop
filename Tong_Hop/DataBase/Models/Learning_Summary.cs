using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataBase.Models
{
    public class Learning_Summary
    {
        [Key]
        public Guid Id { get; set; }
        public double Attendance { get; set; }
        public double Point_15 { get; set; }
        public double Point_45 { get; set; }
        public double Point_Midterm { get; set; }
        public double Point_Final { get; set; }
        public double Point_Summary { get; set; }
        public bool IsView { get; set; } = false;
        [ForeignKey("Id")]
        public Guid? SemesterID { get; set; }
        [ForeignKey("Id")]
        public Guid StudentId { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        public virtual Subjects? Subject { get; set; }
        public virtual Students? Student { get; set; }
        public virtual Semesters? Semester { get; set; }
    }
}
