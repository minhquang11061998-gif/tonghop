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

                var tea = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == classDTO.TeacherId);

                if (grade == null || tea == null)
                {
                    return BadRequest("Lỗi");
                }

                var teaClass = await _db.Classes.FirstOrDefaultAsync(x => x.TeacherId == tea.Id);

                if (teaClass != null)
                {
                    return NotFound("Giáo viên này đã chủ nhiệm 1 lớp rồi");
                }

                string input = classDTO.Name;

                string result = ValidateAndTransformString(input);

                if (result != null)
                {
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
                }
                else
                {
                    Console.WriteLine("Chuỗi không hợp lệ. Vui lòng chỉ nhập chữ cái.");
                }

                return Ok("Them thanh cong");

            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        static string ValidateAndTransformString(string input)
        {
            if (input.Length != 1)
            {
                return "Tên lớp không hợp lệ(ko đc để trống, chỉ nhập 1 ký tự)";
            }

            if (!char.IsLetter(input[0]))
            {
                return "Tến lớp chỉ nhận chữ";
            }
            
            return input.ToUpper();
        }

        [HttpDelete("delete-class")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                var dataStuClass = await _db.Student_Classes.Where(x => x.Id == Id).ToArrayAsync();

                if (dataStuClass.Any())
                {
                    _db.Student_Classes.RemoveRange(dataStuClass);
                    await _db.SaveChangesAsync();
                }

                var dataNotiClass = await _db.Notification_Classes.Where(x => x.Id == Id).ToArrayAsync();

                if (!dataNotiClass.Any())
                {
                    _db.Notification_Classes.RemoveRange(dataNotiClass);
                    await _db.SaveChangesAsync();
                }

                _db.Classes.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Da xoa");
            }

            return BadRequest("Khong co lop nay");
        }

        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClassAndTestCodes(Guid IdClass, string Name, Guid IdTeacher, Guid IdGrade)
        {
            var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == IdClass);

            var grade = await _db.Grades.FirstOrDefaultAsync(x => x.Id == IdGrade);

            var tea = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == IdTeacher);

            if (data == null || grade == null || tea == null)
            {
                return NotFound("Lỗi");
            }

            var teaClass = await _db.Classes.FirstOrDefaultAsync(x => x.TeacherId == tea.Id);

            if (teaClass != null)
            {
                return NotFound("Giáo viên này đã chủ nhiệm 1 lớp rồi");
            }

            string result = ValidateAndTransformString(Name);

            string ClassesName = $"{grade.Name}{result}";

            data.Name = ClassesName;
            data.TeacherId = IdTeacher;

            _db.Classes.Update(data);
            await _db.SaveChangesAsync();

            return Ok("Update class thành công");
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

        [HttpGet("ListSubjectFor")]
        public async Task<ActionResult> GetListSubj(Guid IdClass)
        {
            try
            {
                var listSubj = await (from SubjG in _db.Subject_Grades
                                      join g in _db.Grades on SubjG.GradeId equals g.Id
                                      join cl in _db.Classes on g.Id equals cl.GradeId
                                      join subj in _db.Subjects on SubjG.SubjectId equals subj.Id
                                      where cl.Id == IdClass
                                      select subj).ToListAsync();
                return Ok(listSubj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
