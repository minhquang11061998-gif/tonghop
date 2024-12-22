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

        #region code mới chưa có
        [HttpGet("List_QuestionAndAnswers")]
        public async Task<IActionResult> GetQuestionsAndAnswers(Guid testCodeId)
        {
            try
            {
                var result = await _db.TestCode_TestQuestion
                .Where(tcq => tcq.TestCodeId == testCodeId)
                .Select(tcq => new
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

                int correctAnswers = result.Count(q => q.IsCorrect);// tổng câu đúng 
                int numberOfQuestions = result.Count();
                double totalScore = (10.0 / numberOfQuestions) * correctAnswers;
                totalScore = Math.Round(totalScore, 2); // Làm tròn đến 2 chữ số sau dấu phẩy
                if (!result.Any())
                {
                    return NotFound(new { message = "No questions found for the provided TestCodeId." });
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

        [HttpGet("Exam_results_storage")]
        public async Task<ActionResult> ExamResultsStorage(int CodeTest, double ExamResultStorage, Guid IdStudent)
        {
            try
            {
                var result = await _db.Tests.FirstOrDefaultAsync(x => x.Code == CodeTest);

                var data = new Scores
                {
                    Id = Guid.NewGuid(),
                    Score = ExamResultStorage,
                    PointTypeId = result.PointTypeId,
                    SubjectId = result.SubjectId,
                    StudentId = IdStudent
                };

                _db.Scores.Update(data);
                _db.SaveChanges();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public class ExamHistoriesDTO
        {
            public Guid Id { get; set; }
            public double Score { get; set; }
            public string? Note { get; set; }
            public DateTime CreationTime { get; set; }
            public Guid ExamRoomStudentId { get; set; }
        }

        [HttpGet("Exam_Histories")]
        public async Task<ActionResult> ExamHistories(int CodeTesst, double ExamResultStorage, Guid IdStudent)
        {
            try
            {
                var examroomstudent = await (from a in _db.Tests
                                             join b in _db.Exam_Room_TestCodes on a.Id equals b.TestId
                                             join c in _db.Exam_Room_Students on b.Id equals c.ExamRoomTestCodeId
                                             where a.Code == CodeTesst && c.StudentId == IdStudent
                                             select new
                                             {
                                                 c.Id
                                             }).FirstOrDefaultAsync();

                var data = new ExamHistorys
                {
                    Id = Guid.NewGuid(),
                    Score = ExamResultStorage,
                    Note = "",
                    CreationTime = DateTime.Now,
                    ExamRoomStudentId = examroomstudent.Id
                };

                _db.ExamHistorys.Add(data);
                await _db.SaveChangesAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
