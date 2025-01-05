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
    public class PointTypeDTOController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PointTypeDTOController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetAll_pointtype")]
        public async Task<ActionResult<List<PointTypeDTO>>> GetAll()
        {
            var data = await _db.PointTypes.ToListAsync();

            if (data == null)
            {
                return BadRequest("Danh sách trống");
            }

            var Point = data.Select(x => new PointTypeDTO
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList();

            return Ok(Point);
        }


        [HttpGet("get-by-id-poittype")]
        public async Task<ActionResult<PointTypeDTO>> GetById(Guid id)
        {
            var data = await _db.PointTypes.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return BadRequest("Không tồn tại ID này trong CSDL");
            }

            var poit = new PointTypeDTO
            {
                Id = data.Id,
                Name = data.Name,
            };

            return Ok(poit);
        }


        [HttpPost("create-poittype")]
        public async Task<IActionResult> Create(PointTypeDTO pointTypeDTO)
        {
            var data = new PointTypes
            {
                Id = Guid.NewGuid(),
                Name = pointTypeDTO.Name,
            };

            await _db.AddAsync(data);
            await _db.SaveChangesAsync();

            return Ok("Thêm thành công");
        }

        [HttpPut("update-poittype")]
        public async Task<IActionResult> Update(PointTypeDTO pointTypeDTO)
        {
            var data = await _db.PointTypes.FirstOrDefaultAsync(x => x.Id == pointTypeDTO.Id);

            if (data == null)
            {
                return BadRequest("Không tồn tại Id này trong CSDL");
            }

            data.Name = pointTypeDTO.Name;

            _db.PointTypes.Update(data);
            await _db.SaveChangesAsync();

            return Ok("Cập nhật pointype thành công");
        }

        [HttpDelete("delete-poitype")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _db.PointTypes.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return BadRequest("Không tồn tại Id này trong CSDL");
            }

            _db.Remove(data);
            await _db.SaveChangesAsync();

            return Ok("Xóa thành công");
        }
    }
}
