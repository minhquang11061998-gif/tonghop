using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class Student_User_ClassDTO
    {
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ClassCode { get; set; }
        public string StudentCode { get; set; }
        public string Name { get; set; }
        public DateTime JoinTime { get; set; }
        public int MaxStudent { get; set; }
    }
}
