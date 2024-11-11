using DataBase.Data;
using DataBase.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminIndexController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminIndexController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-bieudo-data")]
        public async Task<List<ChartDTO>> GetGradeData()
        {
            var gradeData = await _db.Grades
                 .Select(g => new ChartDTO
                 {
                     GradeName = g.Name,
                     TotalStudents = g.Class.SelectMany(c => c.Student_Classes).Count(),
                     TotalClasses = g.Class.Count(),
                     TotalTeachers = g.Class.Select(c => c.TeacherId).Distinct().Count()
                 }).ToListAsync();

            return gradeData;
        }
    }
}
