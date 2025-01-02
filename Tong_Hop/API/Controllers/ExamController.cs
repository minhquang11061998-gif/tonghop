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
    public class ExamController : ControllerBase
    {
        private readonly AppDbContext _db;    
        public ExamController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-exam")]
        public async Task<ActionResult<List<ExamDTO>>> GetAll()
        {
            var data = await _db.Exams.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }

            var exam = data.Select(s => new ExamDTO
            {
                Id = s.Id,
                CreationTime = s.CreationTime,
                Status = s.Status,
                SubjectId = s.SubjectId,
            }).ToList();

            return Ok(exam);
        }

        [HttpGet("get-by-id-exam")]
        public async Task<ActionResult<ExamDTO>> GetById(Guid id)
        {
            var data = await _db.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Khong co id nay");
            }

            var exam = new ExamDTO
            {
                Id = data.Id,
                CreationTime = data.CreationTime,
                Status = data.Status,
                SubjectId = data.SubjectId,
            };

            return Ok(exam);
        }

        [HttpPost("create-exam")]
        public async Task<ActionResult> Create(ExamDTO dto)
        {
            try
            {
                var Subj = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == dto.SubjectId);

                if (Subj == null)
                {
                    return NotFound("Không để trống môn học");
                }

                string ExamName = "Bài kiểm tra môn " + Subj.Name;
                var data = new Exams
                {
                    Id = Guid.NewGuid(),
                    Name = ExamName,
                    CreationTime = DateTime.Now,
                    Status = dto.Status,
                    SubjectId = dto.SubjectId,
                };

                await _db.Exams.AddAsync(data);
                await _db.SaveChangesAsync();

                var ExamRoom = new Exam_Room
                {
                    Id = Guid.NewGuid(),
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Status = 1,
                    ExamId=data.Id,
                    RoomId = dto.RoomId,
                    TeacherId1 = dto.TeacherId1,
                    TeacherId2 = dto.TeacherId2,
                };

               await _db.Exam_Rooms.AddAsync(ExamRoom);
               await _db.SaveChangesAsync(true);

                return Ok("Them thanh cong");

            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.ToString()}");
            }
        }

        [HttpPut("update-exam")]
        public async Task<ActionResult> Update(ExamDTO dto)
        {
            try
            {
                var data = await _db.Exams.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (data == null)
                {
                    return NotFound("Khong co đoi tuong nay");
                }

                data.CreationTime = DateTime.Now;
                data.Status = dto.Status;
                data.SubjectId = dto.SubjectId;

                _db.Exams.Update(data);
                _db.SaveChanges();

                return Ok("Update thanh cong");
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpDelete("Delete-exam")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var data = await _db.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (data != null)
            {
                _db.Exams.Remove(data);
                _db.SaveChanges();

                return Ok("Delete thanh cong");
            }

            return BadRequest("Loi");
        }
    }
}
