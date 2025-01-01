using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTOs
{
    public class TestQuestion_TestQuestionAnswersDTO
    {
        public Guid id { get;set; }
        public Guid IdQuestion { get; set; }
        public string QuestionName { get; set; }
        public int QuestionType { get; set; } // Xác định loại câu hỏi
        public int Level { get; set; }
        public string CreatedByName { get; set; }
        public Guid TestId { get; set; }
        public List<string> Answers { get; set; } // Danh sách các đáp án (nếu có)
        public List<string> CorrectAnswers { get; set; } // Danh sách đáp án đúng (cho câu hỏi nhiều đáp án đúng)
    }
    public class TestGridDTO
    {
        public Guid Id { get; set; }
        //public int Type { get; set; }
        public int? Minute { get; set; }
        public int level { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? NumberOfTestCode { get; set; }
        public Guid idquestion { get; set; }
        public int Status { get; set; }
        public Guid ClassId { get; set; }
        public Guid SubjectId { get; set; }
        public string nameclass { get; set; }
        public string SubjectName { get; set; }
        public string Creator { get; set; }
        public string namepoint { get; set; }

    }
    public class TestQuestionDTO
    {
        public Guid Id { get; set; }
        [MaxLength]
        public string QuestionName { get; set; }
        public int code { get; set; }
        public int Type { get; set; }
        [MaxLength]
        public string RightAnswer { get; set; }
        public string CreatedByName { get; set; }
        public int Level { get; set; }
        public Guid? TestId { get; set; }
        public List<AnswerDTO> Answers { get; set; }
    }

    public class AnswerDTO
    {
        public string Answer { get; set; }
        public Guid Id { get; set; }
    }

    public class DetailDTO
    {
        public Guid IdTestcode { get; set; }
        public string CodeTescode { get; set; }
        public int? time { get; set; }
        public List<TestQuestionDTO> NameQuestion { get; set; } // Chỉnh sửa kiểu dữ liệu
        public string NameSubject { get; set; }
        public string Nameclass { get; set; }
        public string codestudent { get; set; }
    }
    public class GetListTestQueryDTO
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? SubjectName { get; set; }
        public int? Type { get; set; }
        public string? Creator { get; set; }
    }
   
    public class PointTypeDto
    {
        public Guid IdPointType { get; set; }
        public int Quantity { get; set; } // Giá trị Quantity tuỳ thuộc vào từng PointType
    }
   
}
