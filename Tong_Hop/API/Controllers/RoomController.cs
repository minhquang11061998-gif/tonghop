using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RoomController(AppDbContext db)
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

        [HttpGet("get-all-room")]
        public async Task<ActionResult<List<RoomDTO>>> GetAll()
        {
            var data = await _db.Rooms.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sách trống");
            }

            var room = data.Select(x => new RoomDTO
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Status = x.Status,
            }).ToList();

            return Ok(room);
        }

        [HttpGet("get-by-id-room")]
        public async Task<ActionResult<RoomDTO>> GetById(Guid id)
        {
            var data = await _db.Rooms.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Không tồn tại ID này trong CSDL");
            }

            var room = new RoomDTO
            {
                Id = data.Id,
                Name = data.Name,
                Code = data.Code,
                Status = data.Status,
            };

            return Ok(room);
        }

        [HttpPost("create-room")]
        public async Task<ActionResult> Create(RoomDTO dto)
        {
            try
            {
                var data = new Rooms
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Code = RamdomCode(8),
                    Status = dto.Status,
                };

                await _db.AddAsync(data);
                _db.SaveChanges();

                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpPut("Update-room")]
        public async Task<ActionResult> Update(RoomDTO dto)
        {
            try
            {

                var data = await _db.Rooms.FirstOrDefaultAsync(x => x.Id == dto.Id);

                if (data == null)
                {
                    return NotFound("Không tồn tại ID này trong CSDL");
                }

                data.Name = dto.Name;
                data.Status = dto.Status;

                _db.Rooms.Update(data);
                _db.SaveChanges();

                return Ok("Cập nhật thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }
    }
}
