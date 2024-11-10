using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class HistDTO
    {
        public Guid TestCodeId { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public List<Question> Questions { get; set; }
    }

    public class Question
    {
        public Guid QuestionId { get; set; }
        public string QuestionName { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class Answer
    {
        public Guid AnswerId { get; set; }
        public string AnswerText { get; set; }
    }
}
