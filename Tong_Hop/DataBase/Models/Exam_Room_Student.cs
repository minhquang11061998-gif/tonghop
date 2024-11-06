using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Exam_Room_Student
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime ChenkTime { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid ExamRoomTestCodeId { get; set; }
        [ForeignKey("Id")]
        public Guid StudentId { get; set; }
        public virtual Exam_Room_TestCode? Exam_Room_TestCode { get; set; }
        public virtual Students? Student { get; set; }
        public virtual ICollection<ExamHistorys>? ExamHistory { get; set; }
        public virtual ICollection<Exam_Room_Student_AnswerHistory>? Exam_Room_Student_AnswerHistory { get; set; }
    }
}
