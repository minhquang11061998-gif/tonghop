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
        public async Task<ActionResult<List<TestDTO>>> GetAll()
        {
            var data = await _db.Tests.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }

            var testdto = data.Select(x => new TestDTO
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                CreationTime = x.CreationTime,
                Minute = x.Minute,
                Maxstudent = x.MaxStudent,
                Status = x.Status,
                SubjectId = x.SubjectId, 
                PointTypeId = x.PointTypeId,
            }).ToList();

            return Ok(testdto);
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
                SubjectId = data.SubjectId,
                PointTypeId = data.PointTypeId,
            };

            return Ok(testdto);
        }

        private int RamdomCodeTest(int length)
        {
            const string CodeNew = "0123456789";

            Random random = new Random();

            char[] code = new char[length];

            for (int i = 0; i < length; i++)
            {
                code[i] = CodeNew[random.Next(CodeNew.Length)];
            }

            return int.Parse(code);
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

        private int GetMaxStudent(string ClassesCode, Guid SubjectId)
        {
            var ClassEntity = _db.Classes.FirstOrDefault(x => x.Code == ClassesCode);

            if (ClassEntity == null)
            {
                Console.WriteLine($"Không tìm thấy lớp với mã: {ClassesCode}");
                return 0;
            }

            var gradeID = ClassEntity.GradeId;

            var subjectGrade = _db.Subject_Grades.FirstOrDefault(x => x.GradeId == gradeID && x.SubjectId == SubjectId);

            if (subjectGrade == null)
            {
                Console.WriteLine($"Không tìm thấy SubjectId {SubjectId} cho GradeId {gradeID}");
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
                if (string.IsNullOrEmpty(testDTO.ClassCode))
                {
                    return NotFound("ClassCode không được để trống.");
                }

                int maxStudents = GetMaxStudent(testDTO.ClassCode, testDTO.SubjectId);
                if (maxStudents == null)
                {
                    return BadRequest("Không tìm thấy số lượng sinh viên tối đa cho lớp học.");
                }

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


    }
}
