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
    }
}
