using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class TestQuestionAnswers
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength]
        public string Answer { get; set; }
        [ForeignKey("Id")]
        public Guid TestQuestionId { get; set; }
        public virtual TestQuestions? TestQuestion { get; set; }
        public virtual ICollection<Exam_Room_Student_AnswerHistory>? Exam_Room_Student_AnswerHistories { get; set; }
    }
}
