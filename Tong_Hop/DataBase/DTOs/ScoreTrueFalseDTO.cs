using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
	public class ScoreTrueFalseDTO
	{
		public int NumberOfQuestions { get; set; } // Tổng số câu hỏi
		public int CorrectAnswers { get; set; }   // Số câu trả lời đúng
		public double TotalScore { get; set; }    // Tổng điểm bài kiểm tra
		public List<QuestionResultDto> Result { get; set; } // Danh sách kết quả từng câu hỏi
	}
	public class QuestionResultDto
	{
		public Guid QuestionId { get; set; }  // Mã định danh câu hỏi
		public string QuestionName { get; set; }  // Nội dung câu hỏi
		public string RightAnswer { get; set; }   // Đáp án đúng
		public List<AnswerDto> Answers { get; set; } // Danh sách các đáp án
		public SelectedAnswerDto SelectedAnswer { get; set; } // Đáp án người dùng chọn
		public bool IsCorrect { get; set; }  // Trạng thái đúng/sai
	}

	public class AnswerDto
	{
		public Guid AnswerId { get; set; }   // Mã định danh đáp án
		public string Answer { get; set; }  // Nội dung đáp án
	}

	public class SelectedAnswerDto
	{
		public Guid AnswerId { get; set; }   // Mã định danh đáp án người dùng chọn
		public string Answer { get; set; }  // Nội dung đáp án người dùng chọn
	}
}
