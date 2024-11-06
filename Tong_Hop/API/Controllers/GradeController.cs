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

        [HttpGet("get-all-grade")]
        public async Task<ActionResult<List<GradeDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Grades.ToArrayAsync();

                if (data == null)
                {
                    return BadRequest("Danh sach trong");
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
                return BadRequest("Loi");
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
                    return BadRequest("Khong co Id nay");
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
                return BadRequest("Loi");
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

                return Ok("Them Thanh Cong");
            }
            catch (Exception)
            {
                return BadRequest("Loi");
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

                return Ok("Update thanh cong");
            };

            return BadRequest("Khong co Id nay");
        }

        [HttpDelete("delete-grade")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Grades.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Grades.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Xoa Thanh cong");
            }

            return BadRequest("Loi");
        }

        [HttpGet("get-grade-data")]
        public async Task<List<GradeDTO>> GetGradeData()
        {
            var totalClass = await _db.Classes.CountAsync();
            var totalTeachers = await _db.Teachers.CountAsync();
            var gradeData = await _db.Grades
                .Select(g => new GradeDTO
                {
                    Name = g.Name,
                    TotalStudents = g.Class.SelectMany(c => c.Student_Classes).Count(),
                    TotalClasses = totalClass,
                    TotalTeachers = totalTeachers
                }).ToListAsync();

            return gradeData;
        }
    }
}
