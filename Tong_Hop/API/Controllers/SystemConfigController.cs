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
    public class SystemConfigController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SystemConfigController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-SystemConfig")]
        public async Task<ActionResult<List<SystemConfigDTO>>> GetAll()
        {
            var data = await _db.SystemConfigs.ToListAsync();

            var sysdto = data.Select(x => new SystemConfigDTO
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                address = x.address,
                IsViewed = x.IsViewed,
                Type = x.Type,
                Value = x.Value,
            }).ToList();
            return Ok(sysdto);
        }

        [HttpGet("get-by-id")]
        public async Task<ActionResult<SystemConfigs>> GetById(Guid Id)
        {
            try
            {
                var data = await _db.SystemConfigs.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return BadRequest("Ko co Id nay");
                }

                var sysdto = new SystemConfigDTO
                {
                    Id = data.Id,
                    Name = data.Name,
                    Email = data.Email,
                    PhoneNumber = data.PhoneNumber,
                    address = data.address,
                    IsViewed = data.IsViewed,
                    Type = data.Type,
                    Value = data.Value,
                };

                return Ok(sysdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create- system")]
        public async Task<IActionResult> Create(SystemConfigDTO sysdto)
        {
            try
            {
                var sys = new SystemConfigs
                {
                    Id = Guid.NewGuid(),
                    Name = sysdto.Name,
                    Email = sysdto.Email,
                    PhoneNumber = sysdto.PhoneNumber,
                    address = sysdto.address,
                    IsViewed = sysdto.IsViewed,
                    Type = 1,
                    Value = sysdto.Value,
                };

                await _db.SystemConfigs.AddAsync(sys);
                await _db.SaveChangesAsync();

                return Ok("Them thanh cong");
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPut("update-system")]
        public async Task<IActionResult> Update(SystemConfigDTO sysdto)
        {
            var data = await _db.SystemConfigs.FirstOrDefaultAsync(x => x.Id == sysdto.Id);

            if (data != null)
            {
                data.Name = sysdto.Name;
                data.Email = sysdto.Email;
                data.PhoneNumber = sysdto.PhoneNumber;
                data.address = sysdto.address;
                data.IsViewed = sysdto.IsViewed;
                data.Type = sysdto.Type;
                data.Value = sysdto.Value;
                _db.SystemConfigs.Update(data);
                await _db.SaveChangesAsync();

                return Ok("Update thanh cong");
            }

            return BadRequest("Loi");
        }

        [HttpDelete("delete-system")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = _db.SystemConfigs.FirstOrDefault(x => x.Id == Id);
            if (data != null)
            {
                _db.SystemConfigs.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Delete thanh cong");
            }

            return BadRequest("Khong co Id nay");
        }

        [HttpPost("mark-as-viewed/{id}")]
        public async Task<IActionResult> MarkAsViewed(Guid id)
        {
            // Tìm kiếm bản ghi trong cơ sở dữ liệu
            var config = await _db.SystemConfigs.FindAsync(id);

            if (config == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }

            // Cập nhật trạng thái IsViewed
            config.IsViewed = true;

            // Lưu thay đổi vào cơ sở dữ liệu
            await _db.SaveChangesAsync();

            return NoContent(); // Trả về 204 No Content nếu thành công
        }
    }
}
