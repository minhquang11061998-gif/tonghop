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
    public class SemestersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SemestersController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<SemesterDTO>>> GetAll()
        {
            var data = await _db.Semesters.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sách trống");
            }

            var dto = data.Select(x => new SemesterDTO
            {
                Id = x.Id,
                Name = x.Name,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
            }).ToList();

            return Ok(dto);
        }

        [HttpGet("get-by-id")]
        public async Task<ActionResult<SemesterDTO>> GetById(Guid id)
        {
            var data = await _db.Semesters.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Không tồn tại ID này trong CSDL");
            }

            var dto = new SemesterDTO
            {
                Id = data.Id,
                Name = data.Name,
                StartTime = data.StartTime,
                EndTime = data.EndTime,
            };

            return Ok(dto);
        }
        
        [HttpPost("create-semester")]
        public async Task<ActionResult> Create(SemesterDTO dto)
        {
            try
            {
                var data = new Semesters
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                };

                await _db.Semesters.AddAsync(data);
                _db.SaveChanges();

                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpPut("update-semester")]
        public async Task<ActionResult> Update(SemesterDTO dto)
        {
            try
            {
                var data = await _db.Semesters.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (data == null)
                {
                    return NotFound("Không tồn tại ID này trong CSDL");
                }

                data.Name = dto.Name;
                data.StartTime = dto.StartTime;
                data.EndTime = dto.EndTime;

                _db.Semesters.Update(data);
                _db.SaveChanges();

                return Ok("Cập nhật thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpDelete("Delete-semesters")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var data = await _db.Semesters.FirstOrDefaultAsync(x => x.Id == id);

            if (data != null)
            {
                _db.Semesters.Remove(data);
                _db.SaveChanges();

                return Ok("Xóa thành công");
            }

            return BadRequest("Đã xảy ra lỗi");
        }
        
    }
}
