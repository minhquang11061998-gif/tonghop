using System.Security.Cryptography;
using Azure.Core;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Emgu.CV;
using Emgu.CV.Features2D;
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
        private readonly Cloudinary _cloud;
        public ClassesController(AppDbContext db, Cloudinary cloud)
        {
            _db = db;
            _cloud = cloud;
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
        [HttpGet("Get-Grade-Class")]
        public IActionResult GetClassesByGrade(Guid gradeId)
        {
            var classes = _db.Classes
                             .Where(c => c.GradeId == gradeId) // Lọc các lớp theo khối
                             .Select(c => new { c.Id, c.Name }) // Lấy Id và tên lớp
                             .ToList();
            return Ok(classes);
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> get()
        {
            var data = await (from a in _db.Classes
                       join b in _db.Teachers on a.TeacherId equals b.Id
                       join c in _db.Users on b.UserId equals c.Id
                       select new ClassStandardDTO
                       {
                           Id = a.Id,
                           Name = a.Name,
                           Code = a.Code,
                           Status = a.Status,
                           MaxStudent = a.MaxStudent,
                           TeacherId = a.TeacherId,
                           GradeId = a.GradeId,
                           NameTeacher = c.FullName,
                       }).ToListAsync();
           
            return Ok(data);
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
        public async Task<ActionResult<ClassStandardDTO>> GetById(Guid Id)
        {

            try
            {
                var data = await (from c in _db.Classes
                                  join t in _db.Teachers on c.TeacherId equals t.Id
                                  join u in _db.Users on t.UserId equals u.Id
                                  where c.Id == Id
                                  select new ClassStandardDTO
                                  {
                                      Id = c.Id,
                                      Code = c.Code,
                                      Name = c.Name.Length > 1 ? c.Name[1].ToString() : string.Empty,
                                      MaxStudent = c.MaxStudent,
                                      Status = c.Status,
                                      TeacherId = c.TeacherId,
                                      GradeId = c.GradeId,
                                      NameTeacher = u.FullName,
                                   
                                  }).FirstOrDefaultAsync();


                return Ok(data);
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
                    string ClassesName = $"{grade.Name}{result}";

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
            using var transaction = await _db.Database.BeginTransactionAsync();
            if (data != null)
            {
                var dataNotiClass = await _db.Notification_Classes.Where(x => x.Id == Id).ToArrayAsync();
                var dataStuClass = await _db.Student_Classes.Where(x => x.ClassId == Id).ToListAsync();
                var studentID = dataStuClass.Select(x => x.StudentId).ToList();
                var student = await _db.Students.Where(x => studentID.Contains(x.Id)).ToListAsync();
                var userId= student.Select(x => x.UserId).ToList();
                var user = await _db.Users.Where(x => userId.Contains(x.Id)).ToListAsync();
                if (dataStuClass.Any())
                {
                    _db.Student_Classes.RemoveRange(dataStuClass);
                   
                }
                if (studentID.Any())
                {
                    _db.Students.RemoveRange(student);
                   
                }
                if(user.Any())
                {
                    
                    foreach (var users in user)
                    {
                        _db.Users.Remove(users);
                        if (!string.IsNullOrEmpty(users.Avartar))
                        {
                            var publicId = users.Avartar.Split('/').Last().Split('.').First();
                            var deletionParams = new DeletionParams(publicId);
                            var deletionResult = await _cloud.DestroyAsync(deletionParams);
                        }
                    }

                }
                if (!dataNotiClass.Any())
                {
                     _db.Notification_Classes.RemoveRange(dataNotiClass);
                }
               
                _db.Classes.Remove(data);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Da xoa");
            }

            return BadRequest("Khong co lop nay");
        }

        //[HttpPut("update-class")]
        //public async Task<IActionResult> updateclass(ClassStandardDTO classs)
        //{
        //    try
        //    {
        //        // Tìm lớp cần cập nhật
        //        var update = await _db.Classes.FirstOrDefaultAsync(x => x.Id == classs.Id);
        //        if (update == null)
        //        {
        //            return NotFound("Lớp không tồn tại");
        //        }

        //        // Lấy tên lớp từ bảng Grades bằng GradeId từ classs
        //        var grade = await _db.Grades.FirstOrDefaultAsync(x => x.Id == classs.GradeId);
        //        if (grade == null)
        //        {
        //            return BadRequest("Lớp không tồn tại trong bảng Grades");
        //        }

        //        // Tạo tên lớp mới
        //        string result = ValidateAndTransformString(classs.Name);
        //        string ClassesName = $"{grade.Name}{result}";
        //        update.Name = ClassesName;
        //        update.Status = classs.Status;
        //        update.TeacherId = classs.TeacherId;
        //        update.GradeId = classs.GradeId;

        //        _db.Classes.Update(update);
        //        await _db.SaveChangesAsync();

        //        return Ok("Cập nhật thành công");
        //    }
        //    catch (Exception ex)        
        //    {
        //        // Log exception nếu cần thiết
        //        return BadRequest($"Cập nhật thất bại: {ex.Message}");
        //    }
        //}

        [HttpPut("update-class")]
        public async Task<IActionResult> UpdateClassAndTestCodes([FromBody] ClassStandardDTO request)
        {
            // Kiểm tra nếu tên lớp trống
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Tên lớp không được để trống.");
            }

            // Lấy thông tin lớp cần cập nhật
            var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == request.Id);
            if (data == null)
            {
                return NotFound("Lớp học không tồn tại.");
            }

            // Kiểm tra nếu grade hoặc teacher không tồn tại
            var grade = await _db.Grades.FirstOrDefaultAsync(x => x.Id == request.GradeId);
            if (grade == null)
            {
                return NotFound("Khối học không tồn tại.");
            }

            var tea = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == request.TeacherId);
            if (tea == null)
            {
                return NotFound("Giáo viên không tồn tại.");
            }

            // Kiểm tra nếu giáo viên đang chủ nhiệm một lớp khác
            var otherClass = await _db.Classes
                .FirstOrDefaultAsync(x => x.TeacherId == request.TeacherId && x.Id != request.Id);
            if (otherClass != null)
            {
                return BadRequest("Giáo viên này đã chủ nhiệm một lớp khác.");
            }

            // Kiểm tra nếu giáo viên đang chủ nhiệm chính lớp này
            if (data.TeacherId == request.TeacherId)
            {
                // Chỉ cập nhật tên lớp
                string result = ValidateAndTransformString(request.Name);
                data.Name = $"{grade.Name}{result}";
            }
            else
            {
                // Cập nhật cả tên lớp và giáo viên
                string result = ValidateAndTransformString(request.Name);
                data.Name = $"{grade.Name}{result}";
                data.TeacherId = request.TeacherId;
            }

            // Cập nhật vào cơ sở dữ liệu
            _db.Classes.Update(data);
            await _db.SaveChangesAsync();

            return Ok("Cập nhật lớp học thành công.");
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
        public async Task<ActionResult> GetListSubj(Guid IdGrade)
        {
            try
            {
                var listSubj = await (from SubjG in _db.Subject_Grades
                                      join g in _db.Grades on SubjG.GradeId equals g.Id
                                      join subj in _db.Subjects on SubjG.SubjectId equals subj.Id
                                      where g.Id == IdGrade
                                      select new {
                                          subj.Id,
                                          subj.Name
                                      } ).ToListAsync();
                return Ok(listSubj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Listteachersubj")]
        public async Task<ActionResult> GetListteacherSubj(Guid idsbj)
        {
            try
            {
                var listSubj = await (from user in _db.Users
                                      join teacher in _db.Teachers on user.Id equals teacher.UserId
                                      join teacher_subject in _db.Teacher_Subjects on teacher.Id equals teacher_subject.TeacherId
                                      join subject in _db.Subjects on teacher_subject.SubjectId equals subject.Id
                                      where subject.Id == idsbj
                                      select new TeacherDTO{
                                        Id = teacher_subject.TeacherId,
                                        Code = teacher.Code,
                                        Anh= user.Avartar,
                                        Name=user.FullName
                }).ToListAsync();
                return Ok(listSubj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
