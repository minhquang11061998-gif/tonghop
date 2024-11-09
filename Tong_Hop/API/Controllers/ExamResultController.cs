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

        [HttpGet("search-student-result")]
        public async Task<ActionResult<IEnumerable<StudentExamResultDTO>>> GetStudentExamResult(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("Keyword is required.");
            }
            // Truy vấn dữ liệu từ các bảng và ánh xạ sang DTO
            var result = await (from student in _db.Students
                                join studentClass in _db.Student_Classes on student.Id equals studentClass.StudentId
                                join classEntity in _db.Classes on studentClass.ClassId equals classEntity.Id
                                join grade in _db.Grades on classEntity.GradeId equals grade.Id
                                join subjectGrade in _db.Subject_Grades on grade.Id equals subjectGrade.GradeId
                                join subject in _db.Subjects on subjectGrade.SubjectId equals subject.Id
                                join exam in _db.Exams on subject.Id equals exam.SubjectId
                                join examRoom in _db.Exam_Rooms on exam.Id equals examRoom.ExamId
                                join examRoomTestCode in _db.Exam_Room_TestCodes on examRoom.Id equals examRoomTestCode.ExamRoomId
                                join examRoomStudent in _db.Exam_Room_Students on examRoomTestCode.Id equals examRoomStudent.ExamRoomTestCodeId
                                join examRoomStudentAnswerHistory in _db.Exam_Room_Student_AnswerHistories on examRoomStudent.Id equals examRoomStudentAnswerHistory.ExamRoomStudentId
                                join testQuestionAnswer in _db.TestQuestionAnswers on examRoomStudentAnswerHistory.TestQuestionAnswerId equals testQuestionAnswer.Id
                                join testQuestion in _db.TestQuestions on testQuestionAnswer.TestQuestionId equals testQuestion.Id
                                join testcodeTestQuestion in _db.TestCode_TestQuestion on testQuestion.Id equals testcodeTestQuestion.TestQuestionId
                                join testcode in _db.TestCodes on testcodeTestQuestion.TestCodeId equals testcode.Id
                                join test in _db.Tests on testcode.TestId equals test.Id
                                join examHistory in _db.ExamHistorys on examRoomStudent.Id equals examHistory.ExamRoomStudentId
                                where student.Code.Contains(keyword) || student.User.FullName.Contains(keyword)
                                select new
                                {
                                    StudentID = student.Id,
                                    examRoomTestCodeId = examRoomTestCode.Id,
                                    GradeName = grade.Name,
                                    ClassName = classEntity.Name,
                                    StudentCode = student.Code,
                                    StudentName = student.User.FullName,
                                    SubjectName = subject.Name,
                                    TestCode = testcode.Code,
                                    RoomName = examRoom.Room.Name,
                                    Answer = testQuestionAnswer.Answer,
                                    RightAnswer = testQuestion.RightAnswer,
                                    ExamTime = examHistory.CreationTime
                                }).ToListAsync();

            // Nhóm kết quả và tính toán số lượng câu trả lời đúng và sai
            var studentResult = result
                .GroupBy(x => new { x.GradeName, x.ClassName, x.StudentID, x.examRoomTestCodeId, x.StudentCode, x.StudentName, x.SubjectName, x.TestCode, x.RoomName, x.ExamTime })
                .Select(g => new StudentExamResultDTO
                {
                    StudentID = g.Key.StudentID,
                    examRoomTestCodeId = g.Key.examRoomTestCodeId,
                    GradeName = g.Key.GradeName,
                    ClassName = g.Key.ClassName,
                    StudentCode = g.Key.StudentCode,
                    StudentName = g.Key.StudentName,
                    SubjectName = g.Key.SubjectName,
                    TestCode = g.Key.TestCode,
                    RoomName = g.Key.RoomName,
                    ExamTime = g.Key.ExamTime,
                    CorrectAnswers = g.Count(x => x.Answer == x.RightAnswer),
                    WrongAnswers = g.Count(x => x.Answer != x.RightAnswer),
                })
                .ToList();

            return Ok(studentResult);
        }

        [HttpGet("get-student-question")]
        public async Task<ActionResult> GetStudentQuestion(Guid studentId, Guid examRoomTestCodeId)
        {
            var result = await (from student in _db.Students
                                join studentClass in _db.Student_Classes on student.Id equals studentClass.StudentId
                                join classEntity in _db.Classes on studentClass.ClassId equals classEntity.Id
                                join grade in _db.Grades on classEntity.GradeId equals grade.Id
                                join subjectGrade in _db.Subject_Grades on grade.Id equals subjectGrade.GradeId
                                join subject in _db.Subjects on subjectGrade.SubjectId equals subject.Id
                                join exam in _db.Exams on subject.Id equals exam.SubjectId
                                join examRoom in _db.Exam_Rooms on exam.Id equals examRoom.ExamId
                                join examRoomTestCode in _db.Exam_Room_TestCodes on examRoom.Id equals examRoomTestCode.ExamRoomId
                                join examRoomStudent in _db.Exam_Room_Students on examRoomTestCode.Id equals examRoomStudent.ExamRoomTestCodeId
                                join examRoomStudentAnswerHistory in _db.Exam_Room_Student_AnswerHistories on examRoomStudent.Id equals examRoomStudentAnswerHistory.ExamRoomStudentId
                                join testQuestionAnswer in _db.TestQuestionAnswers on examRoomStudentAnswerHistory.TestQuestionAnswerId equals testQuestionAnswer.Id
                                join testQuestion in _db.TestQuestions on testQuestionAnswer.TestQuestionId equals testQuestion.Id
                                join testcodeTestQuestion in _db.TestCode_TestQuestion on testQuestion.Id equals testcodeTestQuestion.TestQuestionId
                                join testcode in _db.TestCodes on testcodeTestQuestion.TestCodeId equals testcode.Id
                                join test in _db.Tests on testcode.TestId equals test.Id
                                where student.Id == studentId && examRoomTestCode.Id == examRoomTestCodeId
                                select new TestQuestion_AnswersDTO
                                {
                                    QuestionName = testQuestion.QuestionName,
                                    QuestionType = testQuestion.Type,
                                    Level = testQuestion.Level,
                                    CreatedByName = testQuestion.CreatedByName,
                                    TestId = test.Id,
                                    Answers = _db.TestQuestionAnswers
                                                 .Where(tqAnswer => tqAnswer.TestQuestionId == testQuestion.Id)
                                                 .Select(tqAnswer => tqAnswer.Answer).ToList(),
                                    CorrectAnswers = _db.TestQuestionAnswers
                                                        .Where(tqAnswer => tqAnswer.TestQuestionId == testQuestion.Id
                                                                           && tqAnswer.Answer == testQuestion.RightAnswer)
                                                        .Select(tqAnswer => tqAnswer.Answer).ToList()
                                }).ToListAsync();

            return Ok(result);
        }
    }
}
