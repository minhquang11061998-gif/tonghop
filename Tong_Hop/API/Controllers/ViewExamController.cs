using DataBase.Data;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        
        [HttpGet("test-testcode-question-await")]
        public async Task<IActionResult> Get(int CodeTesst)
        {
            if (CodeTesst == null)
            {
                return NotFound("Không tìm thấy mã bài thi ngẫu nhiên cho TestId đã chọn");
            }

            // Lấy ngẫu nhiên một TestCode dựa trên TestId
            var tc = (from e in _db.TestCodes
                      join f in _db.Tests on e.TestId equals f.Id
                      where f.Code == CodeTesst
                      select e).ToList();


            if (tc.Count == 0)
            {
                return null;
            }
            Random random = new Random();
            // Lấy ngẫu nhiên một phần tử từ danh sách tc
            var randomItem = tc[random.Next(tc.Count)];

            // Bao gồm câu hỏi và đáp án
            var result = new
            {
                TestCodeId = randomItem,
                Code = randomItem.Code,
                Status = randomItem.Status,
                Questions = _db.TestCode_TestQuestion
                    .Where(tctq => tctq.TestCodeId == randomItem.Id)
                    .Select(tctq => new
                    {
                        QuestionId = tctq.TestQuestion.Id,
                        QuestionName = tctq.TestQuestion.QuestionName,
                        Answers = tctq.TestQuestion.TestQuestionAnswer.Select(a => new
                        {
                            AnswerId = a.Id,
                            Answer = a.Answer
                        }).ToList()
                    }).ToList()
            };

            return Ok(result);
        }

        //[HttpGet("Chọn đáp an")]
        //public async Task<ActionResult> CreateHistory(int CodeTesst)
        //{
        //    var examroomstudent = await (from a in _db.Tests
        //                                 join )
        //}
    }
}
