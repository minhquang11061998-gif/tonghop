using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class Exam_Room_Student_DTO
    {
        public Guid Id { get; set; }
        public DateTime ChenkTime { get; set; }
        public int Status { get; set; }
        public Guid ExamRoomTestCodeId { get; set; }
        public Guid StudentId { get; set; }
    }
}
