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

        [HttpGet("GetLearningSummaries")]
        public async Task<IActionResult> GetLearningSummaries(Guid classId, Guid teacherId)
        {
            try
            {
                // Xác định kỳ học hiện tại
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

                // Kiểm tra giáo viên có phải giáo viên chủ nhiệm hay không
                var teacherClass = await _db.Classes
                    .Where(tc => tc.Id == classId && tc.TeacherId == teacherId)
                    .FirstOrDefaultAsync();

                List<Learning_SummaryDTO> summaries;

                if (teacherClass != null)
                {
                    // Tính toán tổng kết nếu giáo viên là chủ nhiệm
                    summaries = await CalculateFinalScoresAsync(classId, currentSemesterId);
                }
                else
                {
                    // Nếu giáo viên không phải là giáo viên chủ nhiệm, lấy danh sách môn học mà giáo viên dạy
                    var studentIdsInClass = await _db.Student_Classes
                        .Where(sc => sc.ClassId == classId)
                        .Select(sc => sc.StudentId)
                        .ToListAsync();

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

                    summaries = await _db.Learning_Summaries
                        .Where(ls => ls.SemesterID == currentSemesterId
                                     && studentIdsInClass.Contains(ls.StudentId)
                                     && subjectIds.Contains(ls.SubjectId))
                        .Select(ls => new Learning_SummaryDTO
                        {
                            Id = ls.Id,
                            StudentId = ls.StudentId,
                            StudentName = _db.Students.Where(s => s.Id == ls.StudentId).Select(s => s.User.FullName).FirstOrDefault(),
                            SubjectId = ls.SubjectId,
                            SubjectName = _db.Subjects.Where(sub => sub.Id == ls.SubjectId).Select(sub => sub.Name).FirstOrDefault(),
                            Attendance = ls.Attendance,
                            Point_15 = ls.Point_15,
                            Point_45 = ls.Point_45,
                            Point_Midterm = ls.Point_Midterm,
                            Point_Final = ls.Point_Final,
                            Point_Summary = ls.Point_Summary,
                            IsView = ls.IsView
                        })
                        .ToListAsync();
                }

                return Ok(summaries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }
        private async Task<List<Learning_SummaryDTO>> CalculateFinalScoresAsync(Guid classId, Guid semesterId)
        {
            var summaries = new List<Learning_SummaryDTO>();

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

                    var Attendance = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Attendance"]).ToList();
                    var point15 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_15"]).ToList();
                    var point45 = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_45"]).ToList();
                    var midterm = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Midterm"]).ToList();
                    var final = subjectScores.Where(s => s.PointTypeId == pointTypeIds["Point_Final"]).ToList();

                    double CalculateAverage(IEnumerable<Scores> points, int quantity)
                    {
                        return quantity > 0 ? points.Sum(p => p.Score) / quantity : 0;
                    }

                    var averageAttendance = CalculateAverage(Attendance, Attendance.Count);
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
                        Attendance = averageAttendance,
                        Point_15 = averagePoint15,
                        Point_45 = averagePoint45,
                        Point_Midterm = averagePointMidterm,
                        Point_Final = averagePointFinal,
                        Point_Summary = (averageAttendance + averagePoint15 + averagePoint45 + averagePointMidterm + averagePointFinal) / 10,
                        IsView = false
                    };

                    summaries.Add(summary);
                }
            }

            return summaries;
        }


    }
}
