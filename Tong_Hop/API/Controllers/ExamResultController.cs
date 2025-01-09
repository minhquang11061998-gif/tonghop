using DataBase.Data;
using DataBase.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamResultController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ExamResultController(AppDbContext db)
        {
            _db = db;
        }

		[HttpGet("get-search-result2")]
		public async Task<ActionResult<IEnumerable<ExamHistoryDTO>>> GetSearchStudentExamResult(string keyword)
		{
			try
			{
				// Truy vấn dữ liệu với LEFT JOIN
				var rawData = await (from examHistory in _db.ExamHistorys

									 join examRoomStudent in _db.Exam_Room_Students
										 on examHistory.ExamRoomStudentId equals examRoomStudent.Id into ersGroup
									 from examRoomStudent in ersGroup.DefaultIfEmpty()

									 join examRoomTestCode in _db.Exam_Room_TestCodes
										 on examRoomStudent.ExamRoomTestCodeId equals examRoomTestCode.Id into ertcGroup
									 from examRoomTestCode in ertcGroup.DefaultIfEmpty()

									 join test in _db.Tests
										 on examRoomTestCode.TestId equals test.Id into testGroup
									 from test in testGroup.DefaultIfEmpty()

									 join examRoomStudentAnswerHistory in _db.Exam_Room_Student_AnswerHistories
										 on examRoomStudent.Id equals examRoomStudentAnswerHistory.ExamRoomStudentId into ersahGroup
									 from examRoomStudentAnswerHistory in ersahGroup.DefaultIfEmpty()

									 join testQuestionAnswer in _db.TestQuestionAnswers
										 on examRoomStudentAnswerHistory.TestQuestionAnswerId equals testQuestionAnswer.Id into tqaGroup
									 from testQuestionAnswer in tqaGroup.DefaultIfEmpty()

									 join testQuestion in _db.TestQuestions
										 on testQuestionAnswer.TestQuestionId equals testQuestion.Id into tqGroup
									 from testQuestion in tqGroup.DefaultIfEmpty()

									 join testcodeTestQuestion in _db.TestCode_TestQuestion
										 on testQuestion.Id equals testcodeTestQuestion.TestQuestionId into tctqGroup
									 from testcodeTestQuestion in tctqGroup.DefaultIfEmpty()

									 join testcode in _db.TestCodes
										 on testcodeTestQuestion.TestCodeId equals testcode.Id into testcodeGroup
									 from testcode in testcodeGroup.DefaultIfEmpty()

									 join examRoom in _db.Exam_Rooms
										 on examRoomTestCode.ExamRoomId equals examRoom.Id into examRoomGroup
									 from examRoom in examRoomGroup.DefaultIfEmpty()

									 join student in _db.Students
										 on examRoomStudent.StudentId equals student.Id into studentGroup
									 from student in studentGroup.DefaultIfEmpty()

									 join studentClass in _db.Student_Classes
										 on student.Id equals studentClass.StudentId into scGroup
									 from studentClass in scGroup.DefaultIfEmpty()

									 join classEntity in _db.Classes
										 on studentClass.ClassId equals classEntity.Id into classGroup
									 from classEntity in classGroup.DefaultIfEmpty()

									 join subject in _db.Subjects
										 on test.SubjectId equals subject.Id into subjectGroup
									 from subject in subjectGroup.DefaultIfEmpty()

										 // Điều kiện tìm kiếm (nếu có keyword)
									 where string.IsNullOrEmpty(keyword) || (student.User.FullName.Contains(keyword))

									 select new ExamHistoryDTO
									 {
										 Id = examHistory.Id,
										 Score = examHistory.Score,
										CreationTime = examHistory.CreationTime,
                                         StudentCode = student != null ? student.Code : "N/A",
										 StudentName = student != null ? student.User.FullName : "N/A",
										 TestName = test != null ? test.Name : "N/A",
										 TestCode = testcode != null ? testcode.Code : "N/A",
										 ClassName = classEntity != null ? classEntity.Name : "N/A",
										 SubjectName = subject != null ? subject.Name : "N/A"
									 }).ToListAsync();

				// Loại bỏ trùng lặp thủ công dựa trên Id
				var distinctResult = rawData
					.GroupBy(x => x.Id) // Nhóm theo Id
					.Select(g => g.First()) // Chọn bản ghi đầu tiên trong nhóm
					.ToList();

				Console.WriteLine($"Số bản ghi sau khi loại bỏ trùng: {distinctResult.Count}");

				return Ok(distinctResult);
			}
			catch (Exception ex)
			{
				return BadRequest($"Lỗi truy vấn: {ex.Message}");
			}
		}


		[HttpGet("Get_Exam_Detail")]
		public async Task<IActionResult> GetQuestionsAndAnswers(string testCode)
		{
			try
			{
				var result = await (from tcq in _db.TestCode_TestQuestion
									join tc in _db.TestCodes on tcq.TestCodeId equals tc.Id
									where tc.Code == testCode // Điều kiện so sánh mã TestCode
									select new
									{
										QuestionId = tcq.TestQuestionId,
										QuestionName = tcq.TestQuestion.QuestionName,
										RightAnswer = tcq.TestQuestion.RightAnswer,
										Answers = _db.TestQuestionAnswers
											.Where(qa => qa.TestQuestionId == tcq.TestQuestionId)
											.Select(qa => new
											{
												AnswerId = qa.Id,
												Answer = qa.Answer
											}).ToList(),
										SelectedAnswer = _db.Exam_Room_Student_AnswerHistories
											.Where(ah => _db.TestQuestionAnswers
												.Where(qa => qa.TestQuestionId == tcq.TestQuestionId)
												.Select(qa => qa.Id)
												.Contains(ah.TestQuestionAnswerId))
											.Select(ah => new
											{
												AnswerId = ah.TestQuestionAnswerId,
												Answer = ah.TestQuestionAnswer.Answer
											}).FirstOrDefault(),
										IsCorrect = _db.Exam_Room_Student_AnswerHistories
											.Where(ah => _db.TestQuestionAnswers
												.Where(qa => qa.TestQuestionId == tcq.TestQuestionId)
												.Select(qa => qa.Id)
												.Contains(ah.TestQuestionAnswerId))
											.Select(ah => ah.TestQuestionAnswer.Answer)
											.FirstOrDefault() == tcq.TestQuestion.RightAnswer
									})
									.ToListAsync();

				int correctAnswers = result.Count(q => q.IsCorrect); // Tổng câu đúng
				int numberOfQuestions = result.Count(); // Tổng số câu hỏi
				double totalScore = (10.0 / numberOfQuestions) * correctAnswers; // Điểm tổng
				totalScore = Math.Round(totalScore, 2); // Làm tròn đến 2 chữ số sau dấu phẩy

				if (!result.Any())
				{
					return NotFound(new { message = "No questions found for the provided TestCode." });
				}

				return Ok(new
				{
					numberOfQuestions = numberOfQuestions,
					correctAnswers = correctAnswers,
					totalScore = totalScore,
					result = result
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}


		[HttpGet("get-result-by-class")]
		public async Task<ActionResult<IEnumerable<ExamHistoryDTO>>> GetStudentExamResult2(Guid classId)
		{
			try
			{
				// Truy vấn dữ liệu với LEFT JOIN
				var rawData = await (from examHistory in _db.ExamHistorys

									 join examRoomStudent in _db.Exam_Room_Students
										 on examHistory.ExamRoomStudentId equals examRoomStudent.Id into ersGroup
									 from examRoomStudent in ersGroup.DefaultIfEmpty()

									 join examRoomTestCode in _db.Exam_Room_TestCodes
										 on examRoomStudent.ExamRoomTestCodeId equals examRoomTestCode.Id into ertcGroup
									 from examRoomTestCode in ertcGroup.DefaultIfEmpty()

									 join test in _db.Tests
										 on examRoomTestCode.TestId equals test.Id into testGroup
									 from test in testGroup.DefaultIfEmpty()

									 join examRoomStudentAnswerHistory in _db.Exam_Room_Student_AnswerHistories on examRoomStudent.Id equals examRoomStudentAnswerHistory.ExamRoomStudentId
									 join testQuestionAnswer in _db.TestQuestionAnswers on examRoomStudentAnswerHistory.TestQuestionAnswerId equals testQuestionAnswer.Id
									 join testQuestion in _db.TestQuestions on testQuestionAnswer.TestQuestionId equals testQuestion.Id
									 join testcodeTestQuestion in _db.TestCode_TestQuestion on testQuestion.Id equals testcodeTestQuestion.TestQuestionId

									 join testcode in _db.TestCodes on testcodeTestQuestion.TestCodeId equals testcode.Id into testcodeGroup
									 from testcode in testcodeGroup.DefaultIfEmpty()

									 join examRoom in _db.Exam_Rooms
										 on examRoomTestCode.ExamRoomId equals examRoom.Id into examRoomGroup
									 from examRoom in examRoomGroup.DefaultIfEmpty()

									join exam in _db.Exams
										on examRoom.ExamId equals exam.Id into examGroup
									from exam in examGroup.DefaultIfEmpty()

									 join student in _db.Students
										 on examRoomStudent.StudentId equals student.Id into studentGroup
									 from student in studentGroup.DefaultIfEmpty()

									 join studentClass in _db.Student_Classes
										 on student.Id equals studentClass.StudentId into scGroup
									 from studentClass in scGroup.DefaultIfEmpty()

									 join classEntity in _db.Classes
										 on studentClass.ClassId equals classEntity.Id into classGroup
									 from classEntity in classGroup.DefaultIfEmpty()

									 join subject in _db.Subjects
										 on test.SubjectId equals subject.Id into subjectGroup
									 from subject in subjectGroup.DefaultIfEmpty()

									 where classEntity.Id == classId // Lọc theo lớp học

									 select new ExamHistoryDTO
									 {
										 Id = examHistory.Id,
										 Score = examHistory.Score,
                                         CreationTime = examHistory.CreationTime,
                                         StudentCode = student != null ? student.Code : "N/A",
										 StudentName = student != null ? student.User.FullName : "N/A",
										 TestName = test != null ? test.Name : "N/A",
										 TestCode = testcode != null ? testcode.Code : "N/A",
										 ClassName = classEntity != null ? classEntity.Name : "N/A",
										 SubjectName = subject != null ? subject.Name : "N/A"
									 }).ToListAsync();

				// Loại bỏ trùng lặp thủ công dựa trên Id
				var distinctResult = rawData
					.GroupBy(x => x.Id) // Nhóm theo Id
					.Select(g => g.First()) // Chọn bản ghi đầu tiên trong nhóm
					.ToList();

				Console.WriteLine($"Số bản ghi sau khi loại bỏ trùng: {distinctResult.Count}");

				return Ok(distinctResult);
			}
			catch (Exception ex)
			{
				return BadRequest($"Lỗi truy vấn: {ex.Message}");
			}
		}
	}
}
