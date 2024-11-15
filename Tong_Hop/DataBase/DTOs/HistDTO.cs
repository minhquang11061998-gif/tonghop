using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class infomationDTO
    {
        public string Namesubject { get; set; }
        public string nametesst {  get; set; }
        public string codesubject { get; set; }
        public int timeexam { get; set; }
        public string namestudent { get; set; }
        public string codestudent { get; set; }
        public string email { get; set; }
    }
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
        public int type { get; set; }
        public string QuestionName { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class Answer
    {
        public Guid AnswerId { get; set; }
        public string AnswerText { get; set; }
    }
}
