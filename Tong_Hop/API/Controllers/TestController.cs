using Data.DTOs;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TestController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-test")]
        public async Task<ActionResult<List<TestDTO>>> GetAll(Guid id)
        {
            var data = await (from test in _db.Tests
                              join examtestcode in _db.Exam_Room_TestCodes on test.Id equals examtestcode.TestId
                              join examroom in _db.Exam_Rooms on examtestcode.ExamRoomId equals examroom.Id
                              where examroom.Id == id select new TestDTO
                              {
                                  Id = test.Id,
                                  Name = test.Name,
                                  Code = test.Code,
                                  CreationTime = test.CreationTime,
                                  Minute = test.Minute,
                                  Maxstudent = test.MaxStudent,
                                  Status = test.Status,
                                  ClassId = test.ClassId,
                                  SubjectId = test.SubjectId,
                                  PointTypeId = test.PointTypeId,
                                  ExamRoomId=examroom.Id,
                              }).ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }
            return Ok(data);
        }
        [HttpGet("get-ByID-test")]
        public async Task<ActionResult<List<TestDTO>>> GetByid(Guid id)
        {
            var data = await (from test in _db.Tests
                              join examtestcode in _db.Exam_Room_TestCodes on test.Id equals examtestcode.TestId
                              join examroom in _db.Exam_Rooms on examtestcode.ExamRoomId equals examroom.Id
                              where test.Id == id
                              select new TestDTO
                              {
                                  Id = test.Id,
                                  Name = test.Name,
                                  Code = test.Code,
                                  CreationTime = test.CreationTime,
                                  Minute = test.Minute,
                                  Maxstudent = test.MaxStudent,
                                  Status = test.Status,
                                  ClassId = test.ClassId,
                                  SubjectId = test.SubjectId,
                                  PointTypeId = test.PointTypeId,
                                  ExamRoomId = examroom.Id,
                              }).FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }
            return Ok(data);
        }
        [HttpGet("{testId}/questions")]
        public async Task<IActionResult> GetQuestions(Guid testId, [FromQuery] int level)
        {
            var questions = await _db.TestQuestions
                .Where(q => q.TestId == testId && q.Level == level)
                .Select(q => new TestQuestion_TestQuestionAnswersDTO
                {

                    TestId = q.Id,
                    QuestionName = q.QuestionName,
                    Level = q.Level,
                    CorrectAnswers = new List<string>(),
                })
                .ToListAsync();

            if (questions == null || !questions.Any())
            {
                return NotFound(new { message = "Không có câu hỏi nào cho test này." });
            }

            return Ok(questions);
        }

        [HttpGet("get-list-test")]
        public async Task<List<TestGridDTO>> GetListTest()
        {
            var query = _db.Tests
                .Include(c => c.testQuestions)
                .Include(c => c.testCodes)
                .Include(t => t.Subject)
                .Include(t => t.Subject.Subject_Grade)
                .ThenInclude(t => t.Grade)
                .ThenInclude(g => g.Class)
                .Include(t => t.PointType)
                .AsQueryable();

            var testList = await query
                .Select(t => new TestGridDTO
                {
                    idquestion = t.testCodes.FirstOrDefault().Id,
                    Id = t.Id,
                    namepoint = t.PointType.Name,
                    nameclass = t.Subject.Subject_Grade
                        .SelectMany(sg => sg.Grade.Class.Select(c => c.Name))
                        .FirstOrDefault(),
                    Name = t.Name,
                    Code = t.Code.ToString(),
                    SubjectName = t.Subject.Name,
                    Status = t.Status,
                    
                    ClassId = t.ClassId,
                    SubjectId = t.SubjectId,
                    Minute = t.Minute,
                })
                .ToListAsync();

            return testList;
        }

        [HttpGet("get-by-id-test")]
        public async Task<ActionResult<TestDTO>> GetById(Guid id)
        {
            var data = await _db.Tests.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Khong co bai thi nay");
            }

            var testdto = new TestDTO
            {
                Id = id,
                Name = data.Name,
                Code = data.Code,
                CreationTime = data.CreationTime,
                Minute = data.Minute,
                Maxstudent = data.MaxStudent,
                Status = data.Status,
                ClassId = data.ClassId,
                SubjectId = data.SubjectId,
                PointTypeId = data.PointTypeId,
            };

            return Ok(testdto);
        }

        private string RamdomCodeTest(int length)
        {
            const string CodeNew = "0123456789";

            Random random = new Random();

            char[] code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = CodeNew[random.Next(CodeNew.Length)];
            }

            return new string(code);
        }

        private string RamdomCodeTestCode(int length)
        {
            const string CodeNew = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Random random = new Random();

            char[] code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = CodeNew[random.Next(CodeNew.Length)];
            }

            return new string(code);
        }

        private int GetMaxStudent(Guid ClassesCode)
        {
            var ClassEntity = _db.Classes.FirstOrDefault(x => x.Id == ClassesCode);

            if (ClassEntity == null)
            {
                Console.WriteLine($"Không tìm thấy lớp với mã: {ClassesCode}");
                return 0;
            }

            return ClassEntity.MaxStudent;
        }

        [HttpPost("create-test-testcode-examroomtestcode")]
        public async Task<IActionResult> CreateTest_Testcode(TestDTO testDTO)
        {
            try
            {
                // Kiểm tra nếu classCode có tồn tại
                if (testDTO.ClassId == null)
                {
                    return NotFound("ClassCode không được để trống.");
                }

                int maxStudents = GetMaxStudent(testDTO.ClassId);
                if (maxStudents == null)
                {
                    return BadRequest("Không tìm thấy số lượng sinh viên tối đa cho lớp học.");
                }

				var testCount = await (from t in _db.Tests
									   join pts in _db.PointType_Subjects on t.PointTypeId equals pts.PointTypeId
									   join subj in _db.Subjects on pts.SubjectId equals subj.Id
									   join p in _db.PointTypes on pts.PointTypeId equals p.Id
									   join gs in _db.Subject_Grades on subj.Id equals gs.SubjectId
									   join g in _db.Grades on gs.GradeId equals g.Id
									   join cl in _db.Classes on g.Id equals cl.GradeId
									   where t.SubjectId == testDTO.SubjectId
											 && t.PointTypeId == testDTO.PointTypeId
											 && cl.Id == testDTO.ClassId
									   select t.Id).CountAsync();

				var maxQuantity = await (from pts in _db.PointType_Subjects
										 where pts.SubjectId == testDTO.SubjectId
											   && pts.PointTypeId == testDTO.PointTypeId
										 select pts.Quantity).FirstOrDefaultAsync();


				if (testCount < maxQuantity)
				{
					// Tạo thực thể Test từ DTO
					var newTest = new Tests
					{
						Id = Guid.NewGuid(),
						Name = testDTO.Name,
						Code = RamdomCodeTest(6), // Tạo mã ngẫu nhiên
						CreationTime = DateTime.Now,
						Minute = testDTO.Minute,
						Status = testDTO.Status,
						SubjectId = testDTO.SubjectId,
						PointTypeId = testDTO.PointTypeId,
                        ClassId = testDTO.ClassId,
						MaxStudent = maxStudents,
					};

					// Thêm thực thể Test vào DbContext
					await _db.Tests.AddAsync(newTest);
					await _db.SaveChangesAsync();

					// Tạo TestCode tương ứng với số lượng MaxStudent
					for (int i = 0; i < maxStudents; i++)
					{
						var newTestCode = new TestCodes
						{
							Id = Guid.NewGuid(),
							Code = RamdomCodeTestCode(8), // Tạo mã ngẫu nhiên
							Status = 1,
							TestId = newTest.Id, // Gán TestId
						};

						// Thêm thực thể TestCode vào DbContext
						await _db.TestCodes.AddAsync(newTestCode);
					}

					var ExamRoomTest = new Exam_Room_TestCode
					{
						Id = Guid.NewGuid(),
						TestId = newTest.Id,
						ExamRoomId = testDTO.ExamRoomId,
					};

					_db.Exam_Room_TestCodes.Add(ExamRoomTest);
					await _db.SaveChangesAsync();
					// Lưu thay đổi vào cơ sở dữ liệu
					await _db.SaveChangesAsync();

					return Ok("Tạo bài kiểm tra và mã bài kiểm tra thành công.");
				}

                return BadRequest("Số lượng bài thi đã đủ");
            }
            catch (Exception ex)
            {

                if (ex.InnerException != null)
                {
                    return BadRequest($"Lỗi khi tạo bài kiểm tra: {ex.InnerException.Message}");
                }

                // Bắt lỗi cụ thể và trả về phản hồi lỗi chi tiết
                return BadRequest($"Lỗi khi tạo bài kiểm tra: {ex.Message}");
            }
        }
        [HttpPut("update-test-testcode")]
        public async Task<IActionResult> UpdateTest_Testcode( TestDTO testDTO)
        {
            try
            {
                // Tìm bài kiểm tra cần cập nhật
                var existingTest = await _db.Tests.FirstOrDefaultAsync(t => t.Id == testDTO.Id);
                if (existingTest == null)
                {
                    return NotFound("Không tìm thấy bài kiểm tra.");
                }

                // Cập nhật thông tin bài kiểm tra
                existingTest.Name = testDTO.Name;
                existingTest.Minute = testDTO.Minute;
                existingTest.SubjectId = testDTO.SubjectId;
                existingTest.PointTypeId = testDTO.PointTypeId;
                existingTest.ClassId = testDTO.ClassId;
                existingTest.MaxStudent = GetMaxStudent(testDTO.ClassId);

                // Kiểm tra và cập nhật số lượng mã bài kiểm tra
                var currentTestCodes = await _db.TestCodes.Where(tc => tc.TestId == testDTO.Id).ToListAsync();
                int currentCount = currentTestCodes.Count;
                int maxStudents = existingTest.MaxStudent;

                if (currentCount < maxStudents)
                {
                    // Thêm mã bài kiểm tra mới nếu số lượng hiện tại nhỏ hơn MaxStudent
                    for (int i = currentCount; i < maxStudents; i++)
                    {
                        var newTestCode = new TestCodes
                        {
                            Id = Guid.NewGuid(),
                            Code = RamdomCodeTestCode(8), // Tạo mã ngẫu nhiên
                            Status = 1,
                            TestId = testDTO.Id,
                        };
                        await _db.TestCodes.AddAsync(newTestCode);
                    }
                }
                else if (currentCount > maxStudents)
                {
                    // Xóa mã bài kiểm tra dư thừa nếu số lượng hiện tại lớn hơn MaxStudent
                    var excessTestCodes = currentTestCodes.Skip(maxStudents).ToList();
                    _db.TestCodes.RemoveRange(excessTestCodes);
                }

                // Cập nhật thông tin Exam_Room_TestCode nếu cần
                var existingExamRoomTestCode = await _db.Exam_Room_TestCodes.FirstOrDefaultAsync(ertc => ertc.TestId == testDTO.Id);
                if (existingExamRoomTestCode != null)
                {
                    existingExamRoomTestCode.ExamRoomId = testDTO.ExamRoomId;
                }
                else if (testDTO.ExamRoomId != null)
                {
                    // Nếu không tồn tại, tạo mới
                    var newExamRoomTestCode = new Exam_Room_TestCode
                    {
                        Id = Guid.NewGuid(),
                        TestId = testDTO.Id,
                        ExamRoomId = testDTO.ExamRoomId,
                    };
                    await _db.Exam_Room_TestCodes.AddAsync(newExamRoomTestCode);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _db.SaveChangesAsync();

                return Ok("Cập nhật bài kiểm tra và mã bài kiểm tra thành công.");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest($"Lỗi khi cập nhật bài kiểm tra: {ex.InnerException.Message}");
                }

                // Bắt lỗi cụ thể và trả về phản hồi lỗi chi tiết
                return BadRequest($"Lỗi khi cập nhật bài kiểm tra: {ex.Message}");
            }
        }

        [HttpDelete("delete-test")]
        public async Task<IActionResult> Delete_test(Guid id)
        {
            var test = await _db.Tests.FirstOrDefaultAsync(x => x.Id == id);
            if (test == null) return NotFound("Test không tồn tại.");
            var testCodes = await _db.TestCodes.Where(x => x.TestId == id).ToListAsync();
            var testQuestions = await _db.TestQuestions.Where(x => x.TestId == id).ToListAsync();
            if (testQuestions.Any()) _db.TestQuestions.RemoveRange(testQuestions);
            if (testCodes.Any()) _db.TestCodes.RemoveRange(testCodes);
            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
            return Ok("Đã xóa thành công.");
        }



    }
}
