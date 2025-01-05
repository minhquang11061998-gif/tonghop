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
    public class Exam_RoomController : ControllerBase
    {
        private readonly AppDbContext _db;
        public Exam_RoomController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-exam-room")]
        public async Task<ActionResult<List<Exam_RoomDTO>>> GetAll()
        {
            var data = await _db.Exam_Rooms.ToListAsync();

            if (data == null)
            {
                return NotFound("Dang sách trống");
            }

            var dto = data.Select(x => new Exam_RoomDTO
            {
                Id = x.Id,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Status = x.Status,
                RoomId = x.RoomId,
                ExamId = x.ExamId,
                TeacherId1 = x.TeacherId1,
                TeacherId2 = x.TeacherId2,
            }).ToList();

            return Ok(dto);

        }

        [HttpGet("get-by-id-room")]
        public async Task<ActionResult<Exam_RoomDTO>> GetById(Guid id)
        {
            var data = await _db.Exam_Rooms.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Không tồn tại ID này trong CSDL");
            }

            var dto = new Exam_RoomDTO 
            {
                Id = data.Id,
                StartTime = data.StartTime,
                EndTime = data.EndTime,
                Status = data.Status,
                RoomId = data.RoomId,
                ExamId = data.ExamId,
                TeacherId1 = data.TeacherId1,
                TeacherId2 = data.TeacherId2,
            };

            return Ok(dto);
        }

        #region ko dùng
        [HttpPost("create-exam-room")]
        public async Task<ActionResult> Create(Exam_RoomDTO dto)
        {
            try
            {
                var data = new Exam_Room
                {
                    Id = Guid.NewGuid(),
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Status = dto.Status,
                    RoomId = dto.RoomId,
                    ExamId = dto.ExamId,
                    TeacherId1 = dto.TeacherId1,
                    TeacherId2 = dto.TeacherId2,
                };

                await _db.Exam_Rooms.AddAsync(data);
                _db.SaveChanges();

                return Ok("Them thanh cong");
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }
        #endregion

        [HttpPut("update-exam-room")]
        public async Task<ActionResult> Update(Exam_RoomDTO dto)
        {
            try
            {
                var data = await _db.Exam_Rooms.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (data == null)
                {
                    return NotFound("Không tồn tại ID này");
                }

                data.Status = dto.Status;
                data.EndTime = dto.EndTime;
                data.Status = dto.Status;
                data.RoomId = dto.RoomId;
                data.ExamId = dto.ExamId;
                data.TeacherId1 = dto.TeacherId1;
                data.TeacherId2 = dto.TeacherId2;

                _db.Exam_Rooms.Update(data);
                _db.SaveChanges();

                return Ok("Cập nhật dữ liệu thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpDelete("delete-exam-room")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var data = await _db.Exam_Rooms.FirstOrDefaultAsync(x => x.Id == id);

            if (data != null)
            {
                _db.Exam_Rooms.Remove(data);
                _db.SaveChanges();

                return Ok("Xóa dữ liệu thành công");
            }

            return BadRequest("Đã xảy ra lỗi");
        }
    }
}
