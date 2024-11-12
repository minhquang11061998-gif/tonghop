using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTOs
{
    public class StudentClassDTO
    {
        public Guid Id { get; set; }
        public DateTime JoinTime { get; set; }
        public int Status { get; set; }
        public Guid ClassId { get; set; }
        public Guid StudentId { get; set; }
    }
}
