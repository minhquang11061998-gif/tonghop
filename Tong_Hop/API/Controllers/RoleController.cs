using CloudinaryDotNet.Actions;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Role = DataBase.Models.Role;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RoleController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-role")]
        public async Task<ActionResult<List<RoleDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Roles.ToListAsync();

                if (data == null)
                {
                    return NotFound("Danh sach trong");
                }

                var role = data.Select(s => new RoleDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Status = s.Status,
                }).ToList();

                return Ok(role);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpGet("get-by-id-role")]
        public async Task<ActionResult<RoleDTO>> GetById(Guid id)
        {
            try
            {
                var data = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return NotFound("Khong co Id nay");
                }

                var role = new RoleDTO
                {
                    Id = data.Id,
                    Name = data.Name,
                    Status = data.Status
                };

                return Ok(role);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create-role")]
        public async Task<ActionResult> Create(RoleDTO dto)
        {
            try
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Status = dto.Status
                };

                await _db.Roles.AddAsync(role);
                await _db.SaveChangesAsync();

                return Ok("Them thanh cong");
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPut("update-role")]
        public async Task<IActionResult> Update(RoleDTO dto)
        {
            var data = await _db.Roles.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (data != null)
            {
                data.Name = dto.Name;
                data.Status = dto.Status;

                _db.Roles.Update(data);
                await _db.SaveChangesAsync();

                return Ok("Update thanh cong");
            }

            return BadRequest("Khong co Id nay");
        }

        [HttpDelete("delete-role")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Roles.FirstOrDefaultAsync(x => x.Id == Id);
            if (data != null)
            {
                _db.Roles.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Xoa thanh cong");
            }

            return BadRequest("Loi");
        }
    }
}
