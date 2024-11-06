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
    public class ClassesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ClassesController(AppDbContext db)
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

        private string RamdomCode_TestCode(int length)
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

        [HttpGet("get-all-class")]
        public async Task<ActionResult<List<ClassesDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Classes.ToListAsync();

                if (data == null)
                {
                    return BadRequest("Danh sach trong");
                }

                var classdto = data.Select(x => new ClassesDTO
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    MaxStudent = x.MaxStudent,
                    Status = x.Status,
                    TeacherId = x.TeacherId,
                    GradeId = x.GradeId,

                }).ToList();

                return Ok(classdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpGet("get-by-id-class")]
        public async Task<ActionResult<ClassesDTO>> GetById(Guid Id)
        {

            try
            {
                var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return BadRequest("Khong co Id nay");
                }

                var classdto = new ClassesDTO
                {
                    Id = data.Id,
                    Code = data.Code,
                    Name = data.Name,
                    MaxStudent = data.MaxStudent,
                    Status = data.Status,
                    TeacherId = data.TeacherId,
                    GradeId = data.GradeId,
                };

                return Ok(classdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create-class")]
        public async Task<IActionResult> Create(ClassesDTO classDTO)
        {
            try
            {
                var grade = await _db.Grades.FirstOrDefaultAsync(x => x.Id == classDTO.GradeId);

                if (grade == null)
                {
                    return BadRequest("Khong tim thay khoi");
                }

                string ClassesName = $"{grade.Name}{classDTO.Name}";

                var data = new Classes
                {
                    Id = Guid.NewGuid(),
                    Code = RamdomCode(8),
                    Name = ClassesName,
                    MaxStudent = classDTO.MaxStudent,
                    Status = classDTO.Status,
                    TeacherId = classDTO.TeacherId,
                    GradeId = classDTO.GradeId,
                };

                await _db.Classes.AddAsync(data);
                await _db.SaveChangesAsync();

                return Ok("Them thanh cong");

            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpDelete("delete-class")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Classes.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Classes.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Da xoa");
            }

            return BadRequest("Khong co lop nay");
        }


    }
}
