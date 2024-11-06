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
    public class PointType_SubjectController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PointType_SubjectController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("get-all-PointType_Subject")]
        public async Task<ActionResult<List<PointType_SubjectDTO>>> GetAll()
        {
            var data = await _db.PointType_Subjects.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sách trống");
            }

            var PTS = data.Select(x => new PointType_SubjectDTO
            {
                Id = x.Id,
                SubjectId = x.SubjectId,
                PointTypeId = x.PointTypeId,
                Quantity = x.Quantity,
            }).ToList();

            return Ok(PTS);
        }

        [HttpGet("get-by-id-PointType_Subject")]
        public async Task<ActionResult<PointType_SubjectDTO>> GetById(Guid id)
        {
            var data = await _db.PointType_Subjects.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Không có Id này");
            }

            var PTS = new PointType_SubjectDTO
            {
                Id = data.Id,
                SubjectId = data.SubjectId,
                PointTypeId = data.PointTypeId,
                Quantity = data.Quantity,
            };

            return Ok(PTS);
        }

        [HttpPost("create-pointtype_subject")]
        public async Task<IActionResult> Create_PointType_Subject(PointType_SubjectDTO dto)
        {
            var data = new PointType_Subject
            {
                Id = Guid.NewGuid(),
                SubjectId = dto.SubjectId,
                PointTypeId = dto.PointTypeId,
                Quantity = dto.Quantity,
            };

            await _db.PointType_Subjects.AddAsync(data);
            await _db.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpPut("update-pointtype_subject")]
        public async Task<IActionResult> Update_PointType_Subejct(PointType_SubjectDTO dto)
        {
            var data = await _db.PointType_Subjects.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (data == null) { return NotFound("Không có Id này"); }

            data.SubjectId = dto.SubjectId;
            data.PointTypeId = dto.PointTypeId;
            data.Quantity = dto.Quantity;

            _db.PointType_Subjects.Update(data);
            await _db.SaveChangesAsync();

            return Ok("Update thành công");
        }

        [HttpDelete("Delete_point_Subject")]
        public async Task<IActionResult> Delete_PointType_Subject(Guid id)
        {
            var data = await _db.PointType_Subjects.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null) { return NotFound("Không có Id này"); }

            _db.PointType_Subjects.Remove(data);
            await _db.SaveChangesAsync();

            return Ok("Đã xóa");
        }

        [HttpGet("get_dau_diem")]
        public async Task<IActionResult> GetPointTypeQuantities()
        {
            try
            {
                // Truy vấn dữ liệu từ bảng Subject và PointType_Subjects
                var pointTypeQuantities = await (from subject in _db.Subjects
                                                 join pts in _db.PointType_Subjects on subject.Id equals pts.SubjectId
                                                 join pt in _db.PointTypes on pts.PointTypeId equals pt.Id
                                                 select new PointTypeQuantityDTO
                                                 {
                                                     SubjectName = subject.Name,
                                                     PointTypeName = pt.Name,
                                                     Quantity = pts.Quantity
                                                 }).ToListAsync(); // Sử dụng ToListAsync để truy vấn dữ liệu

                // Trả về dữ liệu dưới dạng JSON
                return Ok(pointTypeQuantities);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetScores")]
        public IActionResult GetScores(Guid studentId) // Thêm tham số studentId
        {
            var result = from s in _db.Scores
                         join subj in _db.Subjects on s.SubjectId equals subj.Id
                         join pt in _db.PointTypes on s.PointTypeId equals pt.Id
                         join pts in _db.PointType_Subjects on pt.Id equals pts.PointTypeId
                         where s.StudentId == studentId // Lọc theo StudentId
                         group s by new { SubjectName = subj.Name, PointTypeName = pt.Name, pts.Quantity } into g
                         select new
                         {
                             SubjectName = g.Key.SubjectName,
                             PointTypeName = g.Key.PointTypeName,
                             Quantity = g.Key.Quantity,
                             Scores = string.Join(", ", g.Select(x => x.Score))
                         };

            return Ok(result.ToList());
        }

        [HttpGet]
        [Route("GetScores_code")]
        public IActionResult GetScores(string code) // Thay đổi tham số thành code
        {
            // Lấy studentId từ code
            var student = _db.Students.FirstOrDefault(s => s.Code == code);
            if (student == null)
            {
                return NotFound("Không tìm thấy sinh viên với mã: " + code);
            }

            var studentId = student.Id;

            var result = from s in _db.Scores
                         join subj in _db.Subjects on s.SubjectId equals subj.Id
                         join pt in _db.PointTypes on s.PointTypeId equals pt.Id
                         join pts in _db.PointType_Subjects on pt.Id equals pts.PointTypeId
                         where s.StudentId == studentId // Lọc theo StudentId
                         group s by new { SubjectName = subj.Name, PointTypeName = pt.Name, pts.Quantity } into g
                         select new
                         {
                             SubjectName = g.Key.SubjectName,
                             PointTypeName = g.Key.PointTypeName,
                             Quantity = g.Key.Quantity,
                             Scores = string.Join(", ", g.Select(x => x.Score))
                         };

            return Ok(result.ToList());
        }
    }
}
