using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewExamController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ViewExamController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("information_exam")]
        public async Task<IActionResult> Getinfor(int code,string id)
        {
            var infor = (
                 from test in _db.Tests
                 join subject in _db.Subjects on test.SubjectId equals subject.Id
                 join examroomtesstcode in _db.Exam_Room_TestCodes on test.Id equals examroomtesstcode.TestId
                 join examroomstudent in _db.Exam_Room_Students on examroomtesstcode.Id equals examroomstudent.ExamRoomTestCodeId
                 join student in _db.Students on examroomstudent.StudentId equals student.Id
                 join user in _db.Users on student.UserId equals user.Id
                 where test.Code == code && student.Id==Guid.Parse(id)
                 select new infomationDTO
                 {
                     nametesst= test.Name,
                     Namesubject = subject.Name,
                     codesubject = subject.Code,
                     timeexam = test.Minute ?? 0, // Xử lý nếu Minute có thể null
                     namestudent = user.FullName,
                     codestudent = student.Code,
                     email = user.Email
                 }
             ).Distinct().ToList();
            return Ok(infor);
        }
        [HttpGet("test-testcode-question-await")]
        public async Task<IActionResult> Get(int CodeTest)
        {
            if (CodeTest == null)
            {
                return NotFound("Không tìm thấy mã bài thi ngẫu nhiên cho TestId đã chọn");
            }

            // Lấy ngẫu nhiên một TestCode dựa trên TestId
            var tc = (from e in _db.TestCodes
                      join f in _db.Tests on e.TestId equals f.Id
                      where f.Code == CodeTest
                      select e).ToList();

            if (tc.Count == 0)
            {
                return NotFound("Không tìm thấy TestCode cho TestId đã chọn");
            }

            Random random = new Random();
            // Lấy ngẫu nhiên một phần tử từ danh sách tc
            var randomItem = tc[random.Next(tc.Count)];

            // Tạo đối tượng HistDTO
            var result = new HistDTO
            {
                TestCodeId = randomItem.Id,
                Code = randomItem.Code,
                Status = randomItem.Status,
                Questions = _db.TestCode_TestQuestion
                    .Where(tctq => tctq.TestCodeId == randomItem.Id)
                    .Select(tctq => new Question
                    {
                        QuestionId = tctq.TestQuestion.Id,
                        QuestionName = tctq.TestQuestion.QuestionName,
                        type= tctq.TestQuestion.Type,
                        Answers = tctq.TestQuestion.TestQuestionAnswer.Select(a => new Answer
                        {
                            AnswerId = a.Id,
                            AnswerText = a.Answer
                        }).ToList()
                    }).ToList()
            };

            return Ok(result);
        }
        [HttpGet("GetExamDuration")]
        public async Task<IActionResult> GetExamDuration(int codeTest)
        {
            var exam = await _db.Tests.FirstOrDefaultAsync(e => e.Code == codeTest);
            if (exam == null)
            {
                return NotFound("Không tìm thấy bài thi.");
            }

            return Ok(exam.Minute); // Trả về thời gian làm bài (phút)
        }
        [HttpGet("Chọn đáp an")]
        public async Task<ActionResult> IdExamroomstudent(int CodeTesst, Guid GuidId)
        {
            var examroomstudent = await (from a in _db.Tests
                                         join testquestion in _db.TestQuestions on a.Id equals testquestion.TestId
                                         join b in _db.Exam_Room_TestCodes on a.Id equals b.TestId
                                         join c in _db.Exam_Room_Students on b.Id equals c.ExamRoomTestCodeId
                                         where a.Code == CodeTesst && c.StudentId == GuidId
                                         select new
                                         {
                                             testquestion.Type,
                                             c.Id
                                         }).ToListAsync();
            return Ok(examroomstudent);
        }

        [HttpPost("create-hist")]
        public async Task<ActionResult> CreateHistories(int CodeTesst, string GuidId, Guid answerId)
        {
            var examroomstudent = await (from a in _db.Tests
                                         join b in _db.Exam_Room_TestCodes on a.Id equals b.TestId
                                         join c in _db.Exam_Room_Students on b.Id equals c.ExamRoomTestCodeId
                                         where a.Code == CodeTesst && c.StudentId == Guid.Parse(GuidId)
                                         select new
                                         {
                                             c.Id
                                         }).FirstOrDefaultAsync();

            if (examroomstudent == null)
            {
                return NotFound("Exam room student not found");
            }

            var hist = new Exam_Room_Student_AnswerHistory
            {
                Id = Guid.NewGuid(),
                TestQuestionAnswerId = answerId,
                ExamRoomStudentId = examroomstudent.Id,
            };
            _db.Exam_Room_Student_AnswerHistories.Add(hist);
           await _db.SaveChangesAsync();
            return Ok("đã lưu");
        }

        [HttpDelete("Delete_hist")]
        public async Task<ActionResult> DeleteHist(int Cotesst, Guid IDQuestion, Guid IDStudent)
        {
            var answerIds = await (from a in _db.Tests
                                   join b in _db.TestQuestions on a.Id equals b.TestId
                                   join c in _db.TestQuestionAnswers on b.Id equals c.TestQuestionId
                                   where a.Code == Cotesst && b.Id == IDQuestion
                                   select c.Id).ToListAsync();

            // Tìm Id của Exam_Room_Student cho sinh viên (IDStudent) cần kiểm tra
            var examRoomStudentId = await (from ers in _db.Exam_Room_Students
                                           join ert in _db.Exam_Room_TestCodes on ers.ExamRoomTestCodeId equals ert.Id
                                           join t in _db.Tests on ert.TestId equals t.Id
                                           where ers.StudentId == IDStudent && t.Code == Cotesst
                                           select ers.Id).FirstOrDefaultAsync();

            if (examRoomStudentId == Guid.Empty)
            {
                return NotFound("Student not found in exam room.");
            }

            // Tìm các bản ghi trong Exam_Room_Student_AnswerHistories khớp với danh sách đáp án và ExamRoomStudentId
            var answerHistories = await _db.Exam_Room_Student_AnswerHistories
                .Where(d => answerIds.Contains(d.TestQuestionAnswerId) && d.ExamRoomStudentId == examRoomStudentId)
                .ToListAsync();

            // Kiểm tra xem có bản ghi nào để xóa không
            if (answerHistories.Any())
            {
                _db.Exam_Room_Student_AnswerHistories.RemoveRange(answerHistories);
                await _db.SaveChangesAsync();
                return Ok("Đã xóa đáp án cũ");
            }

            return NotFound("Không tìm thấy lịch sử câu trả lời. ");
        }
        [HttpGet("check-answer-history")]
        public async Task<ActionResult<int>> CheckAnswerHistory(int codetest, Guid questionId, Guid studentId)
        {
            // Tìm số lượng đáp án đã được lưu cho câu hỏi này và sinh viên này
            var answerHistoryCount = await (from a in _db.Tests
                                            join b in _db.TestQuestions on a.Id equals b.TestId
                                            join c in _db.TestQuestionAnswers on b.Id equals c.TestQuestionId
                                            join d in _db.Exam_Room_Student_AnswerHistories on c.Id equals d.TestQuestionAnswerId
                                            join ers in _db.Exam_Room_Students on d.ExamRoomStudentId equals ers.Id
                                            where a.Code == codetest && b.Id == questionId && ers.StudentId == studentId
                                            select d).CountAsync();

            return Ok(answerHistoryCount);
        }
    }
}
