using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using static System.Formats.Asn1.AsnWriter;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ScoreController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-score")]
        public async Task<ActionResult<List<ScoreDTO>>> GetAll_Scores()
        {
            var data = await _db.Scores.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }

            var listScore = data.Select(x => new Scores
            {
                Id = x.Id,
                StudentId = x.StudentId,
                SubjectId = x.SubjectId,
                PointTypeId = x.PointTypeId,
                Score = x.Score

            }).ToList();

            return Ok(listScore);
        }

        [HttpGet("get_all_score_student")]
        public async Task<ActionResult<List<ScoreDTO>>> GetAll_Score_Student(Guid id)
        {
            var data = await _db.Scores.Where(x => x.StudentId == id).ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }
            var listScoreStudent = data.Select(x => new Scores
            {
                Id = x.Id,
                StudentId = x.StudentId,
                SubjectId = x.SubjectId,
                PointTypeId = x.PointTypeId,
                Score = x.Score

            }).ToList();

            return Ok(listScoreStudent);
        }

        [HttpPost("generate-scores")]
        public async Task<IActionResult> GenerateScores([FromBody] List<ScoreDTO> subjectPointTypeDtos)
        {
            if (subjectPointTypeDtos == null || !subjectPointTypeDtos.Any())
            {
                return BadRequest("Danh sách đầu vào không hợp lệ");
            }

            var scores = new List<Scores>();

            foreach (var dto in subjectPointTypeDtos)
            {
                var pointTypeSubject = await _db.PointType_Subjects
                    .FirstOrDefaultAsync(pts => pts.SubjectId == dto.SubjectId && pts.PointTypeId == dto.PointTypeId);

                if (pointTypeSubject == null)
                {
                    return NotFound($"Không tìm thấy PointTypeSubject với SubjectId {dto.SubjectId} và PointTypeId {dto.PointTypeId}");
                }

                for (int i = 0; i < pointTypeSubject.Quantity; i++)
                {
                    var score = new Scores
                    {
                        Id = Guid.NewGuid(),
                        StudentId = dto.StudentId,
                        SubjectId = dto.SubjectId,
                        PointTypeId = dto.PointTypeId,
                        Score = 0, // Hoặc logic tính điểm của bạn
                    };

                    scores.Add(score);
                }
            }

            await _db.Scores.AddRangeAsync(scores);
            await _db.SaveChangesAsync();

            return Ok(scores);
        }

        [HttpGet]
        [Route("GetScores_code")]
        public IActionResult GetScores(string code)
        {
            try
            {
                // Lấy studentId từ mã sinh viên (code)
                var student = _db.Students.FirstOrDefault(s => s.Code == code);
                if (student == null)
                {
                    return NotFound("Không tìm thấy sinh viên với mã: " + code);
                }

                var studentId = student.Id;

                // Truy vấn danh sách điểm
                var result = from s in _db.Scores
                             join subj in _db.Subjects on s.SubjectId equals subj.Id
                             join pt in _db.PointTypes on s.PointTypeId equals pt.Id
                             where s.StudentId == studentId
                             select new
                             {
                                 SubjectName = subj.Name, // Tên môn học
                                 PointTypeName = pt.Name, // Tên loại điểm (VD: 15 phút, miệng, giữa kỳ, ...)
                                 s.Score // Điểm
                             };

                // Nhóm theo môn học và loại điểm, trả về từng điểm riêng biệt
                var groupedResult = result
                    .GroupBy(x => x.SubjectName)
                    .Select(g => new
                    {
                        SubjectName = g.Key,
                        FifteenMinutes = g.Where(x => x.PointTypeName == "Point_15").Select(x => x.Score).ToList(),
                        Miệng = g.Where(x => x.PointTypeName == "Attendance").Select(x => x.Score).ToList(),
                        FortyFiveMinutes = g.Where(x => x.PointTypeName == "Point_45").Select(x => x.Score).ToList(),
                        MidTerm = g.Where(x => x.PointTypeName == "Point_Midterm").Select(x => x.Score).ToList(),
                        Final = g.Where(x => x.PointTypeName == "Point_Final").Select(x => x.Score).ToList(),
                    }).ToList();

                return Ok(groupedResult);
            }
            catch (Exception ex)
            {
                return BadRequest("Đã xảy ra lỗi: " + ex.Message);
            }
        }



        [HttpGet]
        [Route("GetScores_codeS")]
        public IActionResult GetScoresS(string code)
        {
            try
            {
                // Lấy studentId từ code
                var student = _db.Students.FirstOrDefault(s => s.Code == code);
                if (student == null)
                {
                    return NotFound("Không tìm thấy sinh viên với mã: " + code);
                }

                var studentId = student.Id;

                // Truy vấn danh sách điểm
                var result = from s in _db.Scores
                             join subj in _db.Subjects on s.SubjectId equals subj.Id
                             join pt in _db.PointTypes on s.PointTypeId equals pt.Id
                             where s.StudentId == studentId
                             group s by new { SubjectName = subj.Name, PointTypeName = pt.Name } into g
                             select new
                             {
                                 SubjectName = g.Key.SubjectName, // Tên môn học
                                 PointTypeName = g.Key.PointTypeName, // Tên loại điểm (VD: 15 phút)
                                 Scores = g.Select(x => x.Score).ToList() // Danh sách điểm
                             };

                // Định dạng kết quả
                var response = result.ToList().Select(r => new
                {
                    SubjectName = r.SubjectName,
                    PointTypeName = r.PointTypeName,
                    Scores = string.Join(", ", r.Scores.Select((score, index) => $"Điểm {index + 1}: {score}")) // Ghép chuỗi điểm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Đã xảy ra lỗi: " + ex.Message);
            }
        }

    }
}
