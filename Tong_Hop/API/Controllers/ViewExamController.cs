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
                     Namesubject = subject.Name,
                     codesubject = subject.Code,
                     timeexam = test.Minute ?? 0, // Xử lý nếu Minute có thể null
                     namestudent = user.FullName,
                     codestudent = student.Code,
                     email = user.Email
                 }
             ).Distinct();
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
                        Answers = tctq.TestQuestion.TestQuestionAnswer.Select(a => new Answer
                        {
                            AnswerId = a.Id,
                            AnswerText = a.Answer
                        }).ToList()
                    }).ToList()
            };

            return Ok(result);
        }

        [HttpGet("Chọn đáp an")]
        public async Task<ActionResult> IdExamroomstudent(int CodeTesst, Guid GuidId)
        {
            var examroomstudent = await (from a in _db.Tests
                                         join b in _db.Exam_Room_TestCodes on a.Id equals b.TestId
                                         join c in _db.Exam_Room_Students on b.Id equals c.ExamRoomTestCodeId
                                         where a.Code == CodeTesst && c.StudentId == GuidId
                                         select new
                                         {
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
    }
}
