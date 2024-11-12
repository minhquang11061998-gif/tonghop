using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class GradeDTO
    {
        public Guid Id { get; set; }
        public int Name { get; set; }
        public int Status { get; set; }
        public int TotalStudents { get; set; }
        public int TotalClasses { get; set; }
        public int TotalTeachers { get; set; }
    }
}
