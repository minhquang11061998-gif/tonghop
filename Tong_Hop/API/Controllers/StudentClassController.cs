using Data.DTOs;
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
    public class StudentClassController : ControllerBase
    {
        private readonly AppDbContext _db;

        public StudentClassController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/StudentClasses/get-all
        [HttpGet("get-all")]
        public async Task<ActionResult<List<StudentClassDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Student_Classes.ToListAsync();

                var studentClassDtos = data.Select(x => new StudentClassDTO
                {
                    Id = x.Id,
                    JoinTime = x.JoinTime,
                    Status = x.Status,
                    ClassId = x.ClassId,
                    StudentId = x.StudentId
                }).ToList();

                return Ok(studentClassDtos);
            }
            catch (Exception)
            {
                return BadRequest("Error retrieving student classes.");
            }
        }

        // GET: api/StudentClasses/get-by-id
        [HttpGet("get-by-id")]
        public async Task<ActionResult<StudentClassDTO>> GetById(Guid id)
        {
            try
            {
                var data = await _db.Student_Classes.FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return NotFound("Student class not found.");
                }

                var studentClassDto = new StudentClassDTO
                {
                    Id = data.Id,
                    JoinTime = data.JoinTime,
                    Status = data.Status,
                    ClassId = data.ClassId,
                    StudentId = data.StudentId
                };

                return Ok(studentClassDto);
            }
            catch (Exception)
            {
                return BadRequest("Error retrieving student class.");
            }
        }



        // POST: api/StudentClasses/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(StudentClassDTO studentClassDto)
        {
            var CurrenDateTime = DateTime.UtcNow;
            try
            {
                var newStudentClass = new Student_Class
                {
                    Id = Guid.NewGuid(),
                    JoinTime = CurrenDateTime,
                    Status = studentClassDto.Status,
                    ClassId = studentClassDto.ClassId,
                    StudentId = studentClassDto.StudentId
                };

                await _db.Student_Classes.AddAsync(newStudentClass);
                await _db.SaveChangesAsync();

                return Ok("Student class created successfully.");
            }
            catch (Exception)
            {
                return BadRequest("Error creating student class.");
            }
        }

        // PUT: api/StudentClasses/update
        [HttpPut("update")]
        public async Task<IActionResult> Update(StudentClassDTO studentClassDto)
        {
            try
            {
                var studentClass = await _db.Student_Classes.FirstOrDefaultAsync(sc => sc.Id == studentClassDto.Id);

                if (studentClass == null)
                {
                    return NotFound("Student class not found.");
                }

                studentClass.JoinTime = studentClassDto.JoinTime;
                studentClass.Status = studentClassDto.Status;
                studentClass.ClassId = studentClassDto.ClassId;
                studentClass.StudentId = studentClassDto.StudentId;

                _db.Student_Classes.Update(studentClass);
                await _db.SaveChangesAsync();

                return Ok("Student class updated successfully.");
            }
            catch (Exception)
            {
                return BadRequest("Error updating student class.");
            }
        }

        // DELETE: api/StudentClasses/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var studentClass = await _db.Student_Classes.FirstOrDefaultAsync(sc => sc.Id == id);

                if (studentClass == null)
                {
                    return NotFound("Student class not found.");
                }

                _db.Student_Classes.Remove(studentClass);
                await _db.SaveChangesAsync();

                return Ok("Student class deleted successfully.");
            }
            catch (Exception)
            {
                return BadRequest("Error deleting student class.");
            }
        }


        [HttpGet("get-studentClass/{classId}")]
        public async Task<IActionResult> GetListStudentsByClassId(Guid classId)
        {
            var students = await _db.Student_Classes
                .Where(sc => sc.ClassId == classId)
                .Select(sc => new StudentClassDTO
                {
                    JoinTime = sc.JoinTime,
                    Status = sc.Status,
                    ClassId = sc.ClassId,
                    StudentId = sc.StudentId
                })
                .ToListAsync();

            if (students == null || !students.Any())
            {
                return NotFound("No students found for this class.");
            }

            return Ok(students);
        }


        [HttpGet("get-classID/{classId}")]
        public async Task<IActionResult> GetStudentsByClassId(Guid classId)
        {
            var students = await (from a in _db.Students
                                  join b in _db.Users on a.UserId equals b.Id
                                  join c in _db.Student_Classes on a.Id equals c.StudentId
                                  join d in _db.Classes on c.ClassId equals d.Id
                                  where d.Id == classId
                                  select new Student_User_ClassDTO
                                  {
                                      FullName = b.FullName,
                                      DateOfBirth = b.DateOfBirth,
                                      StudentCode = a.Code, // Đảm bảo a.Code là mã học sinh
                                      ClassCode = d.Code,
                                      Name = d.Name,
                                      JoinTime = c.JoinTime,
                                      MaxStudent = d.MaxStudent

                                  }).ToListAsync();

            if (students.Count <= 0)
            {
                return NotFound("No students found for this class.");
            }

            return Ok(students);
        }

    }
}
