using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class TeacherDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Anh { get; set; }
    }
    public class subject_teacherDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid idteacher { get; set; }
        public Guid idsubject { get; set; }
    }
}
