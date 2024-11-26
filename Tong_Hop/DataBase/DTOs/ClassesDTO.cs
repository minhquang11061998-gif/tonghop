using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class ClassesDTO
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Status { get; set; }
        public int MaxStudent { get; set; }
        public string TeacherName { get; set; }
        public Guid TeacherId { get; set; }
        public Guid GradeId { get; set; }
        public Guid SubjectId { get; set; }
    }
}
