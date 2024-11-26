using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ClassesController(AppDbContext db)
        {
            _db = db;
        }

        private string RamdomCode(int length)
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

        private string RamdomCode_TestCode(int length)
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

        [HttpGet("get-all-class")]
        public async Task<ActionResult<List<ClassesDTO>>> GetAll()
        {
            try
            {
                var data = await (from c in _db.Classes
                                  join t in _db.Teachers on c.TeacherId equals t.Id
                                  join u in _db.Users on t.UserId equals u.Id
                                  where c.TeacherId == t.Id // Ràng buộc TeacherId khớp
                                  select new
                                  {
                                      c.Id,
                                      c.Code,
                                      c.Name,
                                      c.MaxStudent,
                                      c.Status,
                                      c.TeacherId,
                                      c.GradeId,
                                      TeacherName = u.FullName // Lấy tên giáo viên từ bảng Users
                                  }).ToListAsync();

                if (!data.Any())
                {
                    return BadRequest("Danh sách trống");
                }

                var classdto = data.Select(x => new ClassesDTO
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    MaxStudent = x.MaxStudent,
                    Status = x.Status,
                    TeacherId = x.TeacherId,
                    GradeId = x.GradeId,
                    TeacherName = x.TeacherName
                }).ToList();

                return Ok(classdto);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để kiểm tra
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest("Lỗi");
            }
        }

        [HttpGet("get-by-id-class")]
        public async Task<ActionResult<ClassesDTO>> GetById(Guid Id)
        {

            try
            {
                var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return BadRequest("Khong co Id nay");
                }

                var classdto = new ClassesDTO
                {
                    Id = data.Id,
                    Code = data.Code,
                    Name = data.Name,
                    MaxStudent = data.MaxStudent,
                    Status = data.Status,
                    TeacherId = data.TeacherId,
                    GradeId = data.GradeId,
                };

                return Ok(classdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create-class")]
        public async Task<IActionResult> Create(ClassesDTO classDTO)
        {
            try
            {
                var grade = await _db.Grades.FirstOrDefaultAsync(x => x.Id == classDTO.GradeId);

                if (grade == null)
                {
                    return BadRequest("Khong tim thay khoi");
                }

                string ClassesName = $"{grade.Name}{classDTO.Name}";

                var data = new Classes
                {
                    Id = Guid.NewGuid(),
                    Code = RamdomCode(8),
                    Name = ClassesName,
                    MaxStudent = classDTO.MaxStudent,
                    Status = classDTO.Status,
                    TeacherId = classDTO.TeacherId,
                    GradeId = classDTO.GradeId,
                };

                await _db.Classes.AddAsync(data);
                await _db.SaveChangesAsync();

                return Ok("Them thanh cong");

            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpDelete("delete-class")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Classes.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Da xoa");
            }

            return BadRequest("Khong co lop nay");
        }
        [HttpPut("update-class-and-testcodes")]
        public async Task<IActionResult> UpdateClassAndTestCodes(ClassesDTO classDTO)
        {
            try
            {
                // Kiểm tra nếu ClassCode có tồn tại
                if (string.IsNullOrEmpty(classDTO.Code))
                {
                    return BadRequest("ClassCode không được để trống.");
                }

                // Lấy lớp học dựa trên ClassCode
                var classEntity = _db.Classes.FirstOrDefault(c => c.Code == classDTO.Code);

                if (classEntity == null)
                {
                    return NotFound("Không tìm thấy lớp học.");
                }

                // Lấy thông tin Subject_Grade dựa trên GradeId của lớp và SubjectId từ DTO
                var subjectGrade = _db.Subject_Grades
                    .FirstOrDefault(sg => sg.GradeId == classEntity.GradeId && sg.SubjectId == classDTO.SubjectId);

                if (subjectGrade == null)
                {
                    return NotFound("Không tìm thấy Subject_Grade phù hợp với lớp học và môn học.");
                }

                // Cập nhật số lượng MaxStudent mới cho lớp
                classEntity.Name = classDTO.Name;
                classEntity.Status = classDTO.Status;
                classEntity.TeacherId = classDTO.TeacherId;
                classEntity.GradeId = classDTO.GradeId;
                classEntity.MaxStudent = classDTO.MaxStudent;
                _db.Classes.Update(classEntity);

                // Lấy bài kiểm tra liên quan
                var testEntity = _db.Tests.FirstOrDefault(t => t.SubjectId == classDTO.SubjectId);

                if (testEntity == null)
                {
                    return NotFound("Không tìm thấy bài kiểm tra cho lớp học và môn học.");
                }

                // Cập nhật MaxStudent trong bài kiểm tra
                testEntity.MaxStudent = classDTO.MaxStudent;
                _db.Tests.Update(testEntity);

                // Lấy tất cả TestCode của bài kiểm tra
                var ListTestCodes = _db.TestCodes.Where(tc => tc.TestId == testEntity.Id).ToList();

                // Nếu số lượng hiện tại ít hơn MaxStudent, thêm mới TestCode
                if (ListTestCodes.Count < classDTO.MaxStudent)
                {
                    int missingTestCodes = classDTO.MaxStudent - ListTestCodes.Count;

                    for (int i = 0; i < missingTestCodes; i++)
                    {
                        var newTestCode = new TestCodes
                        {
                            Id = Guid.NewGuid(),
                            Code = RamdomCode_TestCode(8), // Tạo mã ngẫu nhiên
                            Status = 1,
                            TestId = testEntity.Id,
                        };

                        await _db.TestCodes.AddAsync(newTestCode);
                    }
                }
                // Nếu số lượng hiện tại nhiều hơn MaxStudent, xóa bớt TestCode
                else if (ListTestCodes.Count > classDTO.MaxStudent)
                {
                    int excessTestCodes = ListTestCodes.Count - classDTO.MaxStudent;

                    var testCodesToRemove = ListTestCodes.TakeLast(excessTestCodes).ToList();
                    _db.TestCodes.RemoveRange(testCodesToRemove);
                }

                // Lưu các thay đổi
                await _db.SaveChangesAsync();

                return Ok("Cập nhật lớp học và số lượng mã bài kiểm tra thành công.");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return BadRequest($"Lỗi khi cập nhật: {ex.InnerException.Message}");
                }

                return BadRequest($"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        [HttpGet("search-class")]
        public async Task<ActionResult<IEnumerable<ClassesDTO>>> SearchClass(string keyword)
        {
            try
            {
                if (string.IsNullOrEmpty(keyword))
                {
                    // Trả về tất cả các lớp nếu không có từ khóa
                    var allClass = await _db.Classes
                        .Include(c => c.Teacher)
                            .ThenInclude(t => t.User)
                        .Select(c => new ClassesDTO
                        {
                            Id = c.Id,
                            Code = c.Code,
                            Name = c.Name,
                            Status = c.Status,
                            MaxStudent = c.MaxStudent,
                            TeacherName = c.Teacher != null && c.Teacher.User != null ? c.Teacher.User.FullName : "Không rõ",
                            TeacherId = c.TeacherId,
                            GradeId = c.GradeId
                        }).ToListAsync();

                    return Ok(allClass);
                }

                // Chuẩn hóa từ khóa tìm kiếm
                var keywordLower = keyword.ToLower();

                // Tìm kiếm dựa trên từ khóa
                var searchClass = await _db.Classes
                    .Include(c => c.Teacher)
                        .ThenInclude(t => t.User)
                    .Where(c => c.Name.ToLower().Contains(keywordLower) ||
                                (c.Teacher != null && c.Teacher.User != null &&
                                 c.Teacher.User.FullName.ToLower().Contains(keywordLower)))
                    .Select(c => new ClassesDTO
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Name = c.Name,
                        Status = c.Status,
                        MaxStudent = c.MaxStudent,
                        TeacherName = c.Teacher != null && c.Teacher.User != null ? c.Teacher.User.FullName : "Không rõ",
                        TeacherId = c.TeacherId,
                        GradeId = c.GradeId
                    }).ToListAsync();

                return Ok(searchClass);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }

        }


    }
}
