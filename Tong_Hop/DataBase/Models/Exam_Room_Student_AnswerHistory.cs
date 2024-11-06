using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Exam_Room_Student_AnswerHistory
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public Guid ExamRoomStudentId { get; set; }
        [ForeignKey("Id")]
        public Guid TestQuestionAnswerId { get; set; }
        public virtual Exam_Room_Student? Exam_Room_Student { get; set; }
        public virtual TestQuestionAnswers? TestQuestionAnswer { get; set; }
    }
}
