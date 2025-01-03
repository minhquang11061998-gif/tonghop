using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class Learning_SummaryDTO
    {
        public Guid Id { get; set; }
        public double Attendance { get; set; }
        public double Point_15 { get; set; }
        public double Point_45 { get; set; }
        public double Point_Midterm { get; set; }
        public double Point_Final { get; set; }
        public double Point_Summary { get; set; }
        public bool IsView { get; set; } = false;
        public string StudentName { get; set; }
        public string SubjectName { get; set; }
        public Guid? SemesterID { get; set; }
		public string SemesterName { get; set; }
		public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid ClassId { get; set; }
        public Dictionary<string, double> SubjectScores { get; set; }
    }
}
