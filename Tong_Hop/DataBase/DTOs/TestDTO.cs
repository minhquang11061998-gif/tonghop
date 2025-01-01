using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class TestDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string Code { get; set; }
        public int? Minute { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int Maxstudent { get; set; }
        public Guid SubjectId { get; set; }
        public Guid PointTypeId { get; set; }
        public Guid ClassId { get; set; }
        public Guid ExamRoomId { get; set; }
    }
}
