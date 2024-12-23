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
    public class Learning_SummaryController : ControllerBase
    {
        private AppDbContext _db;
        public Learning_SummaryController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("CalculateFinalScores")]
        public async Task<IActionResult> CalculateFinalScores(Guid classId, Guid teacherId)
        {
            try
            {
                // Bước 1: Xác định kỳ học hiện tại (Kỳ 1 hoặc Kỳ 2)
                var currentSemester = await _db.Semesters
                    .Where(s => s.StartTime <= DateTime.Now && s.EndTime >= DateTime.Now)
                    .FirstOrDefaultAsync();

                if (currentSemester == null)
                {
                    return BadRequest(new { message = "No active semester found." });
                }

                // Nếu kỳ học hiện tại là Kỳ 1 mà thời gian đã hết, chuyển sang Kỳ 2
                Guid currentSemesterId;
                if (currentSemester.Name == "Kỳ 1" && DateTime.Now > currentSemester.EndTime)
                {
                    var semester2 = await _db.Semesters
                        .Where(s => s.Name == "Kỳ 2")
                        .FirstOrDefaultAsync();

                    if (semester2 != null)
                    {
                        currentSemesterId = semester2.Id;
                    }
                    else
                    {
                        return BadRequest(new { message = "No Semester 2 found." });
                    }
                }
                else
                {
                    currentSemesterId = currentSemester.Id;
                }

                // Bước 2: Kiểm tra giáo viên có phải là giáo viên chủ nhiệm của lớp này không
                var teacherClass = await _db.Classes
                    .Where(tc => tc.Id == classId && tc.TeacherId == teacherId)
                    .FirstOrDefaultAsync();

                if (teacherClass != null)
                {
                    // Nếu giáo viên là giáo viên chủ nhiệm, lấy tổng kết cả lớp

                    var studentIdsInClass = await _db.Student_Classes
                        .Where(sc => sc.ClassId == classId)
                        .Select(sc => sc.StudentId)
                        .ToListAsync();

                    var studentsInClass = await _db.Students
                        .Where(s => studentIdsInClass.Contains(s.Id))
                        .Include(s => s.User)
                        .ToListAsync();

                    var scores = await _db.Scores
                        .Where(s => studentIdsInClass.Contains(s.StudentId))
                        .ToListAsync();

                    var pointTypes = await _db.PointTypes.ToListAsync();
                    var pointTypeIds = pointTypes.ToDictionary(pt => pt.Name, pt => pt.Id);

                    var existingSummaries = await _db.Learning_Summaries
                        .Where(ls => ls.SemesterID == currentSemesterId && studentIdsInClass.Contains(ls.StudentId))
                        .ToListAsync();

                    var summaries = new List<Learning_SummaryDTO>();

                    foreach (var student in studentsInClass)
                    {
                        var studentScores = scores.Where(s => s.StudentId == student.Id).ToList();

                        var groupedScores = studentScores
                            .GroupBy(s => s.SubjectId)
                            .ToList();

                        foreach (var group in groupedScores)
                        {
                            var subjectId = group.Key;
                            var subjectScores = group.ToList();

                            if (!existingSummaries.Any(es => es.StudentId == student.Id && es.SubjectId == subjectId))
                            {
                                var point15 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_15"]).ToList();
                                var point45 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_45"]).ToList();
                                var midterm = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Midterm"]).ToList();
                                var final = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Final"]).ToList();

                                double CalculateAverage(IEnumerable<Scores> points, int quantity)
                                {
                                    return quantity > 0 ? points.Sum(p => p.Score) / quantity : 0;
                                }

                                var averagePoint15 = CalculateAverage(point15, point15.Count);
                                var averagePoint45 = CalculateAverage(point45, point45.Count);
                                var averagePointMidterm = CalculateAverage(midterm, midterm.Count);
                                var averagePointFinal = CalculateAverage(final, final.Count);

                                var summary = new Learning_SummaryDTO
                                {
                                    Id = Guid.NewGuid(),
                                    StudentId = student.Id,
                                    StudentName = student.User.FullName,
                                    SubjectId = subjectId,
                                    SubjectName = await _db.Subjects.Where(sub => sub.Id == subjectId).Select(sub => sub.Name).FirstOrDefaultAsync(),
                                    Point_15 = averagePoint15,
                                    Point_45 = averagePoint45,
                                    Point_Midterm = averagePointMidterm,
                                    Point_Final = averagePointFinal,
                                    Point_Summary = (averagePoint15 * 0.2) + (averagePoint45 * 0.3) + (averagePointMidterm * 0.2) + (averagePointFinal * 0.3),
                                    IsView = false
                                };

                                summaries.Add(summary);
                            }
                        }
                    }

                    if (summaries.Any())
                    {
                        var learningSummaryEntities = summaries.Select(s => new Learning_Summary
                        {
                            Id = s.Id,
                            StudentId = s.StudentId,
                            SubjectId = s.SubjectId,
                            SemesterID = currentSemesterId,
                            Point_15 = s.Point_15,
                            Point_45 = s.Point_45,
                            Point_Midterm = s.Point_Midterm,
                            Point_Final = s.Point_Final,
                            Point_Summary = s.Point_Summary,
                            IsView = s.IsView
                        }).ToList();

                        _db.Learning_Summaries.AddRange(learningSummaryEntities);
                        await _db.SaveChangesAsync();
                    }

                    return Ok(summaries);
                }
                else
                {
                    // Nếu giáo viên không phải là giáo viên chủ nhiệm, lấy môn học mà giáo viên dạy

                    var subjectIds = await (from tc in _db.Classes
                                            join ts in _db.Teacher_Subjects on tc.TeacherId equals ts.TeacherId
                                            where tc.Id == classId && tc.TeacherId == teacherId
                                            select ts.SubjectId)
                                            .ToListAsync();

                    if (!subjectIds.Any())
                    {
                        return BadRequest(new { message = "Teacher does not teach any subject in this class." });
                    }

                    var studentIdsInClass = await _db.Student_Classes
                        .Where(sc => sc.ClassId == classId)
                        .Select(sc => sc.StudentId)
                        .ToListAsync();

                    var studentsInClass = await _db.Students
                        .Where(s => studentIdsInClass.Contains(s.Id))
                        .Include(s => s.User)
                        .ToListAsync();

                    var scores = await _db.Scores
                        .Where(s => studentIdsInClass.Contains(s.StudentId) && subjectIds.Contains(s.SubjectId))
                        .ToListAsync();

                    var pointTypes = await _db.PointTypes.ToListAsync();
                    var pointTypeIds = pointTypes.ToDictionary(pt => pt.Name, pt => pt.Id);

                    var existingSummaries = await _db.Learning_Summaries
                        .Where(ls => ls.SemesterID == currentSemesterId && studentIdsInClass.Contains(ls.StudentId))
                        .ToListAsync();

                    var summaries = new List<Learning_SummaryDTO>();

                    foreach (var student in studentsInClass)
                    {
                        var studentScores = scores.Where(s => s.StudentId == student.Id).ToList();

                        var groupedScores = studentScores
                            .GroupBy(s => s.SubjectId)
                            .ToList();

                        foreach (var group in groupedScores)
                        {
                            var subjectId = group.Key;
                            var subjectScores = group.ToList();

                            if (!existingSummaries.Any(es => es.StudentId == student.Id && es.SubjectId == subjectId))
                            {
                                var point15 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_15"]).ToList();
                                var point45 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_45"]).ToList();
                                var midterm = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Midterm"]).ToList();
                                var final = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Final"]).ToList();

                                double CalculateAverage(IEnumerable<Scores> points, int quantity)
                                {
                                    return quantity > 0 ? points.Sum(p => p.Score) / quantity : 0;
                                }

                                var averagePoint15 = CalculateAverage(point15, point15.Count);
                                var averagePoint45 = CalculateAverage(point45, point45.Count);
                                var averagePointMidterm = CalculateAverage(midterm, midterm.Count);
                                var averagePointFinal = CalculateAverage(final, final.Count);

                                var summary = new Learning_SummaryDTO
                                {
                                    Id = Guid.NewGuid(),
                                    StudentId = student.Id,
                                    StudentName = student.User.FullName,
                                    SubjectId = subjectId,
                                    SubjectName = await _db.Subjects.Where(sub => sub.Id == subjectId).Select(sub => sub.Name).FirstOrDefaultAsync(),
                                    Point_15 = averagePoint15,
                                    Point_45 = averagePoint45,
                                    Point_Midterm = averagePointMidterm,
                                    Point_Final = averagePointFinal,
                                    Point_Summary = (averagePoint15 * 0.2) + (averagePoint45 * 0.3) + (averagePointMidterm * 0.2) + (averagePointFinal * 0.3),
                                    IsView = false
                                };

                                summaries.Add(summary);
                            }
                        }
                    }

                    if (summaries.Any())
                    {
                        var learningSummaryEntities = summaries.Select(s => new Learning_Summary
                        {
                            Id = s.Id,
                            StudentId = s.StudentId,
                            SubjectId = s.SubjectId,
                            SemesterID = currentSemesterId,
                            Point_15 = s.Point_15,
                            Point_45 = s.Point_45,
                            Point_Midterm = s.Point_Midterm,
                            Point_Final = s.Point_Final,
                            Point_Summary = s.Point_Summary,
                            IsView = s.IsView
                        }).ToList();

                        _db.Learning_Summaries.AddRange(learningSummaryEntities);
                        await _db.SaveChangesAsync();
                    }

                    return Ok(summaries);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("GetLearningSummaries")]
        public async Task<IActionResult> GetLearningSummaries(Guid classId, Guid teacherId)
        {
            try
            {
                // Bước 1: Xác định kỳ học hiện tại (Kỳ 1 hoặc Kỳ 2)
                var currentSemester = await _db.Semesters
                    .Where(s => s.StartTime <= DateTime.Now && s.EndTime >= DateTime.Now)
                    .FirstOrDefaultAsync();

                if (currentSemester == null)
                {
                    return BadRequest(new { message = "No active semester found." });
                }

                Guid currentSemesterId = currentSemester.Id;

                // Nếu kỳ học hiện tại là Kỳ 1 mà thời gian đã hết, chuyển sang Kỳ 2
                if (currentSemester.Name == "Kỳ 1" && DateTime.Now > currentSemester.EndTime)
                {
                    var semester2 = await _db.Semesters
                        .Where(s => s.Name == "Kỳ 2")
                        .FirstOrDefaultAsync();

                    if (semester2 != null)
                    {
                        currentSemesterId = semester2.Id;
                    }
                    else
                    {
                        return BadRequest(new { message = "No Semester 2 found." });
                    }
                }

                // Bước 2: Kiểm tra giáo viên có phải là giáo viên chủ nhiệm của lớp này không
                var teacherClass = await _db.Classes
                    .Where(tc => tc.Id == classId && tc.TeacherId == teacherId)
                    .FirstOrDefaultAsync();

                if (teacherClass != null)
                {
                    // Nếu giáo viên là giáo viên chủ nhiệm, lấy danh sách tổng kết cả lớp
                    var studentIdsInClass = await _db.Student_Classes
                        .Where(sc => sc.ClassId == classId)
                        .Select(sc => sc.StudentId)
                        .ToListAsync();

                    var summaries = await _db.Learning_Summaries
                     .Where(ls => ls.SemesterID == currentSemesterId && studentIdsInClass.Contains(ls.StudentId))
                     .Select(ls => new
                     {
                         ls.Id,
                         ls.StudentId,
                         StudentName = _db.Students.Where(s => s.Id == ls.StudentId).Select(s => s.User.FullName).FirstOrDefault(),
                         SubjectScores = new Dictionary<string, double>
                         {
                            {
                                _db.Subjects.Where(sub => sub.Id == ls.SubjectId).Select(sub => sub.Name).FirstOrDefault(),
                                ls.Point_Summary
                            }
                         },
                         ls.IsView,
                         ls.SemesterID
                     })
                     .ToListAsync();

                    return Ok(summaries);
                }
                else
                {
                    // Nếu giáo viên không phải là giáo viên chủ nhiệm, lấy danh sách tổng kết môn học mà giáo viên dạy
                    // Lấy danh sách học sinh thuộc lớp
                    var studentIdsInClass = await _db.Student_Classes
                        .Where(sc => sc.ClassId == classId)
                        .Select(sc => sc.StudentId)
                        .ToListAsync();

                    // Lấy danh sách môn học mà giáo viên dạy (liên quan đến lớp qua học sinh)
                    var subjectIds = await (from ls in _db.Learning_Summaries
                                            join ts in _db.Teacher_Subjects on ls.SubjectId equals ts.SubjectId
                                            where ts.TeacherId == teacherId
                                                  && studentIdsInClass.Contains(ls.StudentId)
                                            select ts.SubjectId)
                                            .Distinct()
                                            .ToListAsync();

                    if (!subjectIds.Any())
                    {
                        return BadRequest(new { message = "Teacher does not teach any subject in this class." });
                    }

                    // Lấy danh sách tổng kết
                    var summaries = await _db.Learning_Summaries
                        .Where(ls => ls.SemesterID == currentSemesterId
                                     && studentIdsInClass.Contains(ls.StudentId)
                                     && subjectIds.Contains(ls.SubjectId))
                        .Select(ls => new
                        {
                            ls.Id,
                            ls.StudentId,
                            StudentName = _db.Students.Where(s => s.Id == ls.StudentId).Select(s => s.User.FullName).FirstOrDefault(),
                            SubjectScores = new Dictionary<string, double>
                            {
                                {
                                    _db.Subjects.Where(sub => sub.Id == ls.SubjectId).Select(sub => sub.Name).FirstOrDefault(),
                                    ls.Point_Summary
                                }
                            },
                            ls.IsView,
                            ls.SemesterID
                        })
                        .ToListAsync();

                    return Ok(summaries);


                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
