
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
    public class GradeController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GradeController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-grade")]
        public async Task<ActionResult<List<GradeDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Grades.ToListAsync();

                if (data == null)
                {
                    return BadRequest("Danh sách trống");
                }
                var gradto = data.Select(x => new GradeDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Status = x.Status,
                    
                }).ToList();

                return Ok(gradto);
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpGet("get-by-id")]
        public async Task<ActionResult<GradeDTO>> GetById(Guid Id)
        {
            try
            {
                var data = await _db.Grades.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return BadRequest("Không tồn tại ID này");
                }

                var gardto = new GradeDTO
                {
                    Id = data.Id,
                    Name = data.Name,
                    Status = data.Status,
                };

                return Ok(gardto);
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpPost("create-grade")]
        public async Task<IActionResult> Create(GradeDTO gradedto)
        {
            try
            {
                var data = new Grades
                {
                    Id = Guid.NewGuid(),
                    Name = gradedto.Name,
                    Status = gradedto.Status,
                };
                
                await _db.Grades.AddAsync(data);
                await _db.SaveChangesAsync();

                return Ok("Thêm thành công");
            }
            catch (Exception)
            {
                return BadRequest("Đã xảy ra lỗi");
            }
        }

        [HttpPut("update-grade")]
        public async Task<IActionResult> Update(GradeDTO gradedto)
        {
            var data = await _db.Grades.FirstOrDefaultAsync(x => x.Id == gradedto.Id);

            if (data != null)
            {
                data.Name = gradedto.Name;
                data.Status = gradedto.Status;
                _db.Grades.Update(data);
                await _db.SaveChangesAsync();

                return Ok("Cập nhật dữ liệu thành công");
            };

            return BadRequest("Không tồn tại ID này");
        }

        [HttpDelete("delete-grade")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Grades.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Grades.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Xóa thành công");
            }

            return BadRequest("Đã xảy ra lỗi");
        }

        [HttpGet("get-classid")]
        public async Task<ActionResult<IEnumerable<ClassesDTO>>> GetClassesByGradeId()
        {
            var classes = await (from c in _db.Classes
                                 where c.GradeId == c.Id
                                 select new ClassesDTO
                                 {
                                     Id = c.Id,
                                     Code = c.Code,
                                     Name = c.Name,
                                 }).ToListAsync();

            if (classes == null || classes.Count == 0)
            {
                return NotFound("Không tìm thấy lớp học nào thuộc khối này.");
            }

            return Ok(classes);
        }
    }
}
