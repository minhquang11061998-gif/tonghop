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
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SubjectController(AppDbContext db)
        {
            _db = db;
        }

        private string RandomCode(int length)
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

        [HttpGet("get-all-subject")]
        public async Task<ActionResult<List<SubjectDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Subjects.ToListAsync();

                if (data == null)
                {
                    return NotFound("Danh sach trong");
                }

                var subjectdto = data.Select(x => new SubjectDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    CreationTime = x.CreationTime,
                    Status = x.Status,

                }).ToList();

                return Ok(subjectdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpGet("get-by-id-subject")]
        public async Task<ActionResult<SubjectDTO>> GetById(Guid Id)
        {
            try
            {
                var data = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return NotFound("Ko co mon nay");
                }

                var subjectdto = new SubjectDTO
                {
                    Id = data.Id,
                    Name = data.Name,
                    Code = data.Code,
                    CreationTime = data.CreationTime,
                    Status = data.Status,
                };

                return Ok(subjectdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create-subject")]
        public async Task<IActionResult> Create(SubjectDTO dto)
        {
            try
            {
                // Bước 1: Tạo Subject mới
                var subj = new Subjects
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Code = RandomCode(8),
                    CreationTime = DateTime.UtcNow,
                    Status = dto.Status,
                };

                // Thêm Subject vào cơ sở dữ liệu
                await _db.Subjects.AddAsync(subj);
                await _db.SaveChangesAsync();

                // Bước 2: Thêm danh sách Subject_Grade dựa trên GradeIds
                foreach (var gradeId in dto.GradeIds)
                {
                    var subjectGrade = new Subject_Grade
                    {
                        Id = Guid.NewGuid(),
                        Status = 1,
                        GradeId = gradeId,
                        SubjectId = subj.Id
                    };

                    await _db.Subject_Grades.AddAsync(subjectGrade);
                    await _db.SaveChangesAsync();
                }

                // Bước 3: Thêm danh sách PointType_Subject dựa trên PointTypeDtos
                foreach (var pointTypeDto in dto.PointTypeIds)
                {
                    var pointTypeSubject = new PointType_Subject
                    {
                        Id = Guid.NewGuid(),
                        SubjectId = subj.Id,
                        PointTypeId = pointTypeDto.IdPointType,
                        Quantity = pointTypeDto.Quantity // Giá trị Quantity nhập từ người dùng
                    };

                    await _db.PointType_Subjects.AddAsync(pointTypeSubject);
                }

                var Tch_subj = new Teacher_Subject
                {
                    Id = Guid.NewGuid(),
                    SubjectId = subj.Id,
                    TeacherId = dto.TeacherId,
                };

                await _db.Teacher_Subjects.AddAsync(Tch_subj);
                _db.SaveChanges();

                // Lưu các PointType_Subject
                await _db.SaveChangesAsync();

                return Ok("Them thanh cong");
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return BadRequest("Loi: " + ex.Message);
            }
        }

        [HttpPut("update-subject")]
        public async Task<IActionResult> Update(SubjectDTO subjectDTO)
        {
            var data = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == subjectDTO.Id);

            if (data != null)
            {
                data.Name = subjectDTO.Name;
                data.CreationTime = DateTime.UtcNow;
                data.Status = subjectDTO.Status;

                _db.Subjects.Update(data);
                await _db.SaveChangesAsync();


                return Ok("Update thanh cong");
            }

            return BadRequest("Loi");
        }

        [HttpDelete("delete-subject")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Subjects.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Xoa thanh cong");
            }

            return BadRequest("Loi");
        }
    }
}
