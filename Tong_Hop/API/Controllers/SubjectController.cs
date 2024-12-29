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
    public class SubjectController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SubjectController(AppDbContext db)
        {
            _db = db;
        }

        private string RandomCode(int length)
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

        [HttpGet("get-all-subject")]
        public async Task<ActionResult<List<SubjectDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Subjects.ToListAsync();

                if (data == null)
                {
                    return NotFound("Danh sach trong");
                }

                var subjectdto = data.Select(x => new SubjectDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Code = x.Code,
                    CreationTime = x.CreationTime,
                    Status = x.Status,

                }).ToList();

                return Ok(subjectdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpGet("get-by-id-subject")]
        public async Task<ActionResult<SubjectDTO>> GetById(Guid Id)
        {
            try
            {
                var data = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == Id);

                if (data == null)
                {
                    return NotFound("Ko co mon nay");
                }

                var subjectdto = new SubjectDTO
                {
                    Id = data.Id,
                    Name = data.Name,
                    Code = data.Code,
                    CreationTime = data.CreationTime,
                    Status = data.Status,
                };

                return Ok(subjectdto);
            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpPost("create-subject")]
        public async Task<IActionResult> Create(SubjectDTO dto)
        {
			try
			{
				// Bước 1: Tạo Subject mới
				var subj = new Subjects
				{
					Id = Guid.NewGuid(),
					Name = dto.Name,
					Code = RandomCode(8),
					CreationTime = DateTime.UtcNow,
					Status = dto.Status,
				};

				// Thêm Subject vào cơ sở dữ liệu
				await _db.Subjects.AddAsync(subj);
				await _db.SaveChangesAsync();

				// Bước 2: Thêm danh sách Subject_Grade dựa trên GradeIds
				foreach (var gradeId in dto.GradeIds)
				{
					var subjectGrade = new Subject_Grade
					{
						Id = Guid.NewGuid(),
						Status = 1,
						GradeId = gradeId,
						SubjectId = subj.Id
					};

					await _db.Subject_Grades.AddAsync(subjectGrade);
					await _db.SaveChangesAsync();
				}
				#region sửa để thêm đồng thồi toàn bộ pointTypes cho các môn
				// Bước 3: Tự động thêm danh sách PointType_Subject dựa trên 5 PointType mặc định
				var defaultPointTypes = _db.PointTypes.ToList(); // Lấy toàn bộ PointType từ cơ sở dữ liệu
				foreach (var pointType in defaultPointTypes)
				{
					int quantity = 0;

					// Xác định Quantity dựa trên PointTypeName
					switch (pointType.Name)
					{
						case "Attendance":
						case "Point_15":
							quantity = 3;
							break;
						case "Point_Midterm":
						case "Point_Final":
							quantity = 1;
							break;
						case "Point_45":
							quantity = 2;
							break;
					}

					var pointTypeSubject = new PointType_Subject
					{
						Id = Guid.NewGuid(),
						SubjectId = subj.Id,
						PointTypeId = pointType.Id,
						Quantity = quantity
					};

					await _db.PointType_Subjects.AddAsync(pointTypeSubject);
				}
				#endregion
				var Tch_subj = new Teacher_Subject
				{
					Id = Guid.NewGuid(),
					SubjectId = subj.Id,
					TeacherId = dto.TeacherId,
				};

				await _db.Teacher_Subjects.AddAsync(Tch_subj);
				await _db.SaveChangesAsync();

				return Ok("Thêm thành công");
			}
			catch (Exception ex)
			{
				// Log lỗi nếu cần
				return BadRequest("Lỗi: " + ex.Message);
			}

		}

		#region đã sửa có thể update môn khối và giáo viên
		[HttpPut("update-subject")]
        public async Task<IActionResult> Update(SubjectDTO subjectDTO)
        {
			try
			{
				// Bước 1: Tìm Subject cần cập nhật
				var subject = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == subjectDTO.Id);
				if (subject == null)
				{
					return NotFound("Subject không tồn tại");
				}

				subject.CreationTime = DateTime.UtcNow; // Cập nhật thời gian
				await _db.SaveChangesAsync();

				//// Bước 2: Cập nhật danh sách Subject_Grade
				//var existingGrades = _db.Subject_Grades.Where(sg => sg.SubjectId == subjectDTO.Id).ToList();
				//var newGradeIds = subjectDTO.GradeIds ?? new List<Guid>();

				//// Xóa các Subject_Grade không còn trong danh sách
				//foreach (var grade in existingGrades)
				//{
				//	if (!newGradeIds.Contains(grade.GradeId))
				//	{
				//		_db.Subject_Grades.Remove(grade);
				//	}
				//}

				//// Thêm các Subject_Grade mới
				//foreach (var gradeId in newGradeIds)
				//{
				//	if (!existingGrades.Any(eg => eg.GradeId == gradeId))
				//	{
				//		var newGrade = new Subject_Grade
				//		{
				//			Id = Guid.NewGuid(),
				//			SubjectId = subjectDTO.Id,
				//			GradeId = gradeId,
				//			Status = 1
				//		};
				//		await _db.Subject_Grades.AddAsync(newGrade);
				//	}
				//}

				//await _db.SaveChangesAsync();
				#region còn có thế dùng chưa xóa đc
				//// Bước 3: Cập nhật danh sách PointType_Subject
				//var existingPointTypes = _db.PointType_Subjects.Where(pt => pt.SubjectId == subjectDTO.Id).ToList();
				//var newPointTypeDtos = subjectDTO.PointTypeIds ?? new List<PointTypeDto>();

				//foreach (var pointType in existingPointTypes)
				//{
				//	if (!newPointTypeDtos.Any(pt => pt.IdPointType == pointType.PointTypeId))
				//	{
				//		_db.PointType_Subjects.Remove(pointType);
				//	}
				//}

				//foreach (var pointTypeDto in newPointTypeDtos)
				//{
				//	var existingPointType = existingPointTypes.FirstOrDefault(pt => pt.PointTypeId == pointTypeDto.IdPointType);
				//	if (existingPointType != null)
				//	{
				//		existingPointType.Quantity = pointTypeDto.Quantity;
				//	}
				//	else
				//	{
				//		var newPointType = new PointType_Subject
				//		{
				//			Id = Guid.NewGuid(),
				//			SubjectId = subjectDTO.Id,
				//			PointTypeId = pointTypeDto.IdPointType,
				//			Quantity = pointTypeDto.Quantity
				//		};
				//		await _db.PointType_Subjects.AddAsync(newPointType);
				//	}
				//}

				//await _db.SaveChangesAsync();
				#endregion
				// Bước 4: Cập nhật Teacher_Subject
				var existingTeacher = await _db.Teacher_Subjects.FirstOrDefaultAsync(ts => ts.SubjectId == subjectDTO.Id);
				if (existingTeacher != null)
				{
					existingTeacher.TeacherId = subjectDTO.TeacherId;
				}
				else
				{
					var newTeacher = new Teacher_Subject
					{
						Id = Guid.NewGuid(),
						SubjectId = subjectDTO.Id,
						TeacherId = subjectDTO.TeacherId
					};
					await _db.Teacher_Subjects.AddAsync(newTeacher);
				}

				await _db.SaveChangesAsync();

				return Ok("Cập nhật thành công");
			}
			catch (Exception ex)
			{
				return BadRequest("Lỗi: " + ex.Message);
			}
		}
		#endregion

		[HttpDelete("delete-subject")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Subjects.FirstOrDefaultAsync(x => x.Id == Id);

            if (data != null)
            {
                _db.Subjects.Remove(data);
                await _db.SaveChangesAsync();

                return Ok("Xoa thanh cong");
            }

            return BadRequest("Loi");
        }
    }
}
