using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class StudentExamResultDTO
    {
        public Guid StudentID { get; set; }
        public Guid examRoomTestCodeId { get; set; }
        public string StudentCode { get; set; }
        public int GradeName { get; set; }
        public string ClassName { get; set; }
        public string StudentName { get; set; }
        public string SubjectName { get; set; }
        public string TestCode { get; set; }
        public string RoomName { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public string Answer { get; set; }
        public string RightAnswer { get; set; }
        public DateTime ExamTime { get; set; }
    }
}
