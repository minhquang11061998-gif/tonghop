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
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TeacherController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("get-all-teacher-all")]
        public async Task<ActionResult<List<TeacherDTO>>> GetAllteacher()
        {
            var data = await (from user in _db.Users
                              join teacher in _db.Teachers on user.Id equals teacher.UserId
                              select new TeacherDTO
                              {
                                  Id = teacher.Id,
                                  Name = user.FullName,
                                  Code = teacher.Code
                              }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("get-all-teacher")]
        public async Task<ActionResult<List<TeacherDTO>>> GetAll()
        {
            var data = await (from user in _db.Users
                              join teacher in _db.Teachers on user.Id equals teacher.UserId
                              where !_db.Classes.Any(c => c.TeacherId == teacher.Id) 
                              select new TeacherDTO
                              {
                                  Id = teacher.Id,
                                  Name = user.FullName,
                                  Code = teacher.Code
                              }).ToListAsync();

            return Ok(data);
        }


        [HttpGet("get-all-teachers-not-assigned-to-any-subject")]
        public async Task<ActionResult<List<TeacherDTO>>> Get()
        {
            // Lấy tất cả giáo viên không được gán cho bất kỳ môn học nào
            var data = await (from teacher in _db.Teachers
                              join user in _db.Users on teacher.UserId equals user.Id
                              where !_db.Teacher_Subjects.Any(ts => ts.TeacherId == teacher.Id) // Giáo viên chưa gán môn học
                              select new TeacherDTO
                              {
                                  Id = teacher.Id,
                                  Name = user.FullName,
                                  Code = teacher.Code
                              }).ToListAsync();

            return Ok(data);
        }



        [HttpGet("get-by-id")]
        public async Task<ActionResult<Teachers>> GetById(Guid id)
        {
            var data = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(data);
        }

        [HttpGet("coute-student")]
        public async Task<IActionResult> CouteStudent()
        {
            var coute = await _db.Teachers.CountAsync();
            return Ok(coute);
        }
    }
}
