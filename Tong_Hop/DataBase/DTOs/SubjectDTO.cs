using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class SubjectDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public List<Guid>? GradeIds { get; set; }
        public Guid TeacherId { get; set; }

    }
}
