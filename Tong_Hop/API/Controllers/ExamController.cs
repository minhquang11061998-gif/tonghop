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
    public class ExamController : ControllerBase
    {
        private readonly AppDbContext _db;    
        public ExamController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("get-all-exam-new")]
        public async Task<ActionResult<GetAllExamCaThiDTO>> GetAllEXAM()
        {
            var data = await (from subject in _db.Subjects
                              join test in _db.Tests on subject.Id equals test.SubjectId
                              join exam in _db.Exams on subject.Id equals exam.SubjectId
                              join examRoom in _db.Exam_Rooms on exam.Id equals examRoom.ExamId
                              join teacher1 in _db.Teachers on examRoom.TeacherId1 equals teacher1.Id
                              join teacher2 in _db.Teachers on examRoom.TeacherId2 equals teacher2.Id
                              join user1 in _db.Users on teacher1.UserId equals user1.Id
                              join user2 in _db.Users on teacher2.UserId equals user2.Id
                              join room in _db.Rooms on examRoom.RoomId equals room.Id
                              select new GetAllExamCaThiDTO
                              {
                                  Id = exam.Id,
                                  IdTest=test.Id,
                                  Name = exam.Name,
                                  NameTeacher1 = user1.FullName,
                                  idteacher1 = examRoom.TeacherId1,
                                  NameTeacher2 = user2.FullName,
                                  idteacher2 = examRoom.TeacherId2,
                                  Nameroom = room.Name,
                                  idrom = examRoom.RoomId,
                                  NameSubject = subject.Name,
                                  idsubject = exam.SubjectId,
                                  CreationTime = exam.CreationTime,
                                  StartTime = examRoom.StartTime,
                                  EndTime = examRoom.EndTime,
                                  IdEaxmRoom = examRoom.Id,
                              }).ToListAsync();

            if (!data.Any())
            {
                return NotFound("Danh sách trống");
            }

            return Ok(data);
        }
        [HttpGet("get-Byid-exam-new")]
        public async Task<ActionResult<GetAllExamDTO>> GetByidEXAM(Guid id)
        {
            var data = await (from subject in _db.Subjects
                              join exam in _db.Exams on subject.Id equals exam.SubjectId
                              join examRoom in _db.Exam_Rooms on exam.Id equals examRoom.ExamId
                              join teacher1 in _db.Teachers on examRoom.TeacherId1 equals teacher1.Id
                              join teacher2 in _db.Teachers on examRoom.TeacherId2 equals teacher2.Id
                              join user1 in _db.Users on teacher1.UserId equals user1.Id
                              join user2 in _db.Users on teacher2.UserId equals user2.Id
                              join room in _db.Rooms on examRoom.RoomId equals room.Id
                              where exam.Id == id
                              select new GetAllExamDTO
                              {
                                  Id = exam.Id,
                                  Name = exam.Name,
                                  NameTeacher1 = user1.FullName,
                                  idteacher1=examRoom.TeacherId1,
                                  NameTeacher2 = user2.FullName,
                                  idteacher2 = examRoom.TeacherId2,
                                  Nameroom = room.Name,
                                  idrom=examRoom.RoomId,
                                  NameSubject = subject.Name,
                                  idsubject=exam.SubjectId,
                                  CreationTime=exam.CreationTime,
                                  StartTime=examRoom.StartTime,
                                  EndTime=examRoom.EndTime,
                              }).FirstOrDefaultAsync();


            return Ok(data);
        }
        [HttpGet("get-all-exam")]
        public async Task<ActionResult<List<ExamDTO>>> GetAll()
        {
            var data = await _db.Exams.ToListAsync();

            if (data == null)
            {
                return NotFound("Danh sach trong");
            }

            var exam = data.Select(s => new ExamDTO
            {
                Id = s.Id,
                CreationTime = s.CreationTime,
                Status = s.Status,
                SubjectId = s.SubjectId,
            }).ToList();

            return Ok(exam);
        }

        [HttpGet("get-by-id-exam")]
        public async Task<ActionResult<ExamDTO>> GetById(Guid id)
        {
            var data = await _db.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Khong co id nay");
            }

            var exam = new ExamDTO
            {
                Id = data.Id,
                CreationTime = data.CreationTime,
                Status = data.Status,
                SubjectId = data.SubjectId,
            };

            return Ok(exam);
        }

        [HttpPut("update-exam")]
        public async Task<ActionResult> Update(GetAllExamDTO dto)
        {
            try
            {
                // Tìm bài kiểm tra theo Id
                var existingExam = await _db.Exams.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (existingExam == null)
                {
                    return NotFound("Không tìm thấy bài kiểm tra.");
                }

                // Cập nhật thông tin bài kiểm tra
                var subj = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == dto.idsubject);
                if (subj == null)
                {
                    return NotFound("Không để trống môn học.");
                }
                existingExam.Name = "Bài kiểm môn " + subj.Name;
                existingExam.Status = dto.Status;
                existingExam.SubjectId = dto.idsubject;
                existingExam.CreationTime = DateTime.Now; // Nếu cần cập nhật thời gian tạo

                // Tìm phòng thi liên quan đến bài kiểm tra
                var existingExamRoom = await _db.Exam_Rooms.FirstOrDefaultAsync(x => x.ExamId == dto.Id);
                if (existingExamRoom == null)
                {
                    return NotFound("Không tìm thấy phòng thi.");
                }
                existingExamRoom.RoomId = dto.idrom;
                existingExamRoom.TeacherId1 = dto.idteacher1;
                existingExamRoom.TeacherId2 = dto.idteacher2;

                // Lưu thay đổi
                _db.Exams.Update(existingExam);
                _db.Exam_Rooms.Update(existingExamRoom);
                await _db.SaveChangesAsync();

                return Ok("Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.ToString()}");
            }
        }
        [HttpPost("create-exam")]
        public async Task<ActionResult> Create(GetAllExamDTO dto)
        {
            try
            {
                var Subj = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == dto.idsubject);

                if (Subj == null)
                {
                    return NotFound("Không để trống môn học");
                }

                string ExamName = "Bài kiểm tra" + Subj.Name;
                var data = new Exams
                {
                    Id = Guid.NewGuid(),
                    Name = ExamName,
                    CreationTime = DateTime.Now,
                    Status = dto.Status,
                    SubjectId = dto.idsubject,
                };

                await _db.Exams.AddAsync(data);
                await _db.SaveChangesAsync();

                var ExamRoom = new Exam_Room
                {
                    Id = Guid.NewGuid(),
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Status = 1,
                    ExamId = data.Id,
                    RoomId = dto.idrom,
                    TeacherId1 = dto.idteacher1,
                    TeacherId2 = dto.idteacher2,
                };

                await _db.Exam_Rooms.AddAsync(ExamRoom);
                await _db.SaveChangesAsync(true);

                return Ok("Them thanh cong");

            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.ToString()}");
            }
        }


        [HttpDelete("Delete-exam")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var data = await _db.Exams.FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return NotFound("Khong co bai kiem tra nay");
            }

            var examRoom = await _db.Exam_Rooms.FirstOrDefaultAsync(x => x.ExamId == id);

            if (examRoom == null)
            {
                return NotFound("Bai kiem tra nay chua thiet lap phong thi");
            }

            _db.Exam_Rooms.Remove(examRoom);
            _db.Exams.Remove(data);
            await _db.SaveChangesAsync();
            return BadRequest("Loi");
        }

        [HttpGet("get-all-exam-cathi")]
        public async Task<ActionResult<GetAllExamCaThiDTO>> GetAllEXAM_Cathi()
        {
            var data = await (from subject in _db.Subjects
                              join exam in _db.Exams on subject.Id equals exam.SubjectId
                              join examRoom in _db.Exam_Rooms on exam.Id equals examRoom.ExamId
                              join teacher1 in _db.Teachers on examRoom.TeacherId1 equals teacher1.Id
                              join teacher2 in _db.Teachers on examRoom.TeacherId2 equals teacher2.Id
                              join user1 in _db.Users on teacher1.UserId equals user1.Id
                              join user2 in _db.Users on teacher2.UserId equals user2.Id
                              join room in _db.Rooms on examRoom.RoomId equals room.Id
                              join examRoomTestCode in _db.Exam_Room_TestCodes on examRoom.Id equals examRoomTestCode.ExamRoomId
                              join Test in _db.Tests on examRoomTestCode.TestId equals Test.Id
                              select new GetAllExamCaThiDTO
                              {
                                  Id = exam.Id,
                                  Name = exam.Name,
                                  Status = exam.Status,
                                  NameTeacher1 = user1.FullName,
                                  idteacher1 = examRoom.TeacherId1,
                                  NameTeacher2 = user2.FullName,
                                  idteacher2 = examRoom.TeacherId2,
                                  Nameroom = room.Name,
                                  idrom = examRoom.RoomId,
                                  NameSubject = subject.Name,
                                  idsubject = exam.SubjectId,
                                  CreationTime = exam.CreationTime,
                                  StartTime = examRoom.StartTime,
                                  EndTime = examRoom.EndTime,
                                  IdEaxmRoom = examRoom.Id,
                                  IdTest = Test.Id,
                                  CodeTest = Test.Code
                              }).ToListAsync();

            if (!data.Any())
            {
                return NotFound("Danh sách trống");
            }

            return Ok(data);
        }
    }
}
