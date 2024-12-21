using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class Exam_Room_Histories_DTO
    {
        public Guid Id { get; set; }
      
        public Guid ExamRoomStudentId { get; set; }
     
        public Guid TestQuestionAnswerId { get; set; }
    }

	public class ExamHistoryDTO
	{
		public Guid Id { get; set; }
		public double Score { get; set; }
		public string? Note { get; set; }
		public DateTime CreationTime { get; set; }
		public Guid ExamRoomStudentId { get; set; }
		public string StudentCode { get; set; }
		public string StudentName { get; set; }
		public string ClassName { get; set; }
		public int GradeName { get; set; }
		public string SubjectName { get; set; }
		public string TestName { get; set; }
		public string TestCode { get; set; }
	}
}
