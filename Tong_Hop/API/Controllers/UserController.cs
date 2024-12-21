using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Database.DTOs;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Emgu.CV;
using Emgu.CV.Features2D;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Cloudinary _cloud;
        private readonly AppDbContext _db;
        public UserController(AppDbContext db,Cloudinary cloud)
        {
            _db = db;
            _cloud = cloud;
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloud.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(new { Url = uploadResult.SecureUrl.ToString() });
            }

            return StatusCode((int)uploadResult.StatusCode, uploadResult.Error.Message);
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

        [HttpGet("get-all-user")]
        public async Task<ActionResult<List<UserDTO>>> GetAll()
        {
            try
            {
                var data = await _db.Users.ToArrayAsync();

                if (data == null)
                {
                    return NotFound("Danh sach trong");
                }

                var UserDTO = data.Select(x => new UserDTO
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Avartar = x.Avartar,
                    Email = x.Email,
                    UserName = x.UserName,
                    PasswordHash = x.PasswordHash,
                    DateOfBirth = x.DateOfBirth,
                    PhoneNumber = x.PhoneNumber,
                    IsLocked = x.IsLocked,
                    LockedEndTime = x.LockedEndTime,
                    CreationTime = x.CreationTime,
                    LastMordificationTime = x.LastMordificationTime,
                    Status = x.Status,
                    RoleId = x.RoleId,

                }).ToList();

                return Ok(UserDTO);

            }
            catch (Exception)
            {
                return BadRequest("Loi");
            }
        }

        [HttpGet("get-by-id-user")]
        public async Task<ActionResult<UserDTO>> GetById(Guid id)
        {
            try
            {
                // Lấy thông tin user dựa trên Id
                var data = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (data == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Truy vấn lớp học liên kết với User
                var idclass = await (from a in _db.Classes
                                     join b in _db.Student_Classes on a.Id equals b.ClassId
                                     join c in _db.Students on b.StudentId equals c.Id
                                     where c.UserId == id
                                     select a.Id).FirstOrDefaultAsync();

                // Tạo DTO để trả về
                var userDTO = new UserDTO
                {
                    Id = data.Id,
                    FullName = data.FullName,
                    Avartar = data.Avartar,
                    Email = data.Email,
                    UserName = data.UserName,
                    PasswordHash = data.PasswordHash,
                    DateOfBirth = data.DateOfBirth,
                    PhoneNumber = data.PhoneNumber,
                    IsLocked = data.IsLocked,
                    LockedEndTime = data.LockedEndTime,
                    CreationTime = data.CreationTime,
                    LastMordificationTime = data.LastMordificationTime,
                    Status = data.Status,
                    RoleId = data.RoleId,
                    idclass = idclass // Gắn giá trị idclass vào DTO
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết hơn (nếu cần)
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

      
        [HttpPost("create-user")]
        public async Task<IActionResult> Create([FromForm] UserDTO user, IFormFile avatarFile,Guid id)
        {
        
            try
            {
                var userId = Guid.NewGuid();
                string avatarPath=null;
                if (avatarFile == null || avatarFile.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(avatarFile.FileName, avatarFile.OpenReadStream()),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await _cloud.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    avatarPath= uploadResult.SecureUrl.ToString();
                }

               
                // Cập nhật thời gian thay đổi cuối cùng
                var currentDateTime = DateTime.UtcNow;

                // Tạo đối tượng User mới với các giá trị mặc định
                var data = new Users
                {
                    Id = userId, // Sử dụng userId vừa tạo
                    FullName = user.FullName,
                    Avartar = avatarPath, // Đường dẫn ảnh lưu trong thuộc tính Avatar
                    Email = user.Email,
                    UserName = user.UserName,
                    PasswordHash = user.PasswordHash,
                    DateOfBirth = user.DateOfBirth ?? DateTime.UtcNow, // Nếu không có, mặc định là hiện tại
                    PhoneNumber = user.PhoneNumber,
                    IsLocked = user.IsLocked,
                    LockedEndTime = user.IsLocked ? (user.LockedEndTime ?? currentDateTime.AddDays(30)) : (DateTime?)null, // Nếu bị khóa, mặc định sau 30 ngày, nếu không thì null
                    CreationTime = currentDateTime, // Mặc định là thời gian hiện tại
                    LastMordificationTime = currentDateTime, // Mặc định là thời gian hiện tại
                    Status = user.Status,
                    RoleId = user.RoleId,
                };

                // Thêm User mới vào database
                await _db.Users.AddAsync(data);
                await _db.SaveChangesAsync();

                // Kiểm tra và tạo thông tin tương ứng cho Student hoặc Teacher
                var role = _db.Roles.FirstOrDefault(x => x.Id == data.RoleId);
                if (role != null)
                {
                    if (role.Name == "Student")
                    {
                        // Thêm học sinh
                        var student = new Students
                        {
                            Id = Guid.NewGuid(),
                            Code = RandomCode(8),
                            UserId = data.Id
                        };
                        await _db.Students.AddAsync(student);
                        await _db.SaveChangesAsync();

                        // Thêm vào Student_Classes
                        var studentClass = new Student_Class
                        {
                            Id = Guid.NewGuid(),
                            JoinTime = DateTime.Now,
                            Status = 1,
                            StudentId = student.Id,
                            ClassId = id
                        };
                        await _db.Student_Classes.AddAsync(studentClass);
                        await _db.SaveChangesAsync();
                        await updateclass(id);

                    }
                    else if (role.Name == "Teacher")
                    {
                        var teacher = new Teachers
                        {
                            Id = Guid.NewGuid(),
                            Code = RandomCode(8),
                            UserId = data.Id
                        };
                        await _db.Teachers.AddAsync(teacher);
                        await _db.SaveChangesAsync();
                    }
                   
                }

                return Ok("Them thanh cong");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> Update([FromForm]UserDTO userDTO, IFormFile newImage)
        {
            var data = await _db.Users.FirstOrDefaultAsync(x => x.Id == userDTO.Id);

            if (data == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            // Cập nhật thông tin người dùng
            data.FullName = userDTO.FullName;
            data.Email = userDTO.Email;
            data.UserName = userDTO.UserName;
            data.PasswordHash = userDTO.PasswordHash;
            data.DateOfBirth = userDTO.DateOfBirth;
            data.PhoneNumber = userDTO.PhoneNumber;
            data.IsLocked = userDTO.IsLocked;
            data.LockedEndTime = userDTO.LockedEndTime;
            data.CreationTime = userDTO.CreationTime;
            data.LastMordificationTime = DateTime.UtcNow;
            data.Status = userDTO.Status;
            data.RoleId = userDTO.RoleId;

            // Nếu có hình ảnh mới, tải lên Cloudinary và cập nhật đường dẫn
            if (newImage != null && newImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(data.Avartar))
                {
                    var publicId = Path.GetFileNameWithoutExtension(data.Avartar); // Lấy public ID từ URL
                    var deleteParams = new DeletionParams(publicId); // Tạo tham số xóa ảnh
                    var deletionResult = await _cloud.DestroyAsync(deleteParams); // Xóa ảnh cũ trên Cloudinary

                    if (deletionResult.StatusCode != HttpStatusCode.OK)
                    {
                        return BadRequest("Không thể xóa ảnh cũ trên Cloudinary.");
                    }
                }

                // Tải ảnh lên Cloudinary
                using (var stream = new MemoryStream())
                {
                    await newImage.CopyToAsync(stream);
                    stream.Position = 0; // Đặt lại vị trí stream về đầu để tải lên Cloudinary

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(newImage.FileName, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
                       
                    };

                    var uploadResult = await _cloud.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode != HttpStatusCode.OK)
                    {
                        return BadRequest("Cập nhật ảnh thất bại trên Cloudinary.");
                    }

                    // Cập nhật đường dẫn hình ảnh trong cơ sở dữ liệu
                    data.Avartar = uploadResult.SecureUrl.ToString(); // Lưu đường dẫn hình ảnh mới
                }
            }
            else
            {
                // Nếu không có hình ảnh mới, giữ nguyên ảnh cũ
                data.Avartar = userDTO.Avartar;
            }

            // Cập nhật người dùng trong cơ sở dữ liệu
            _db.Users.Update(data);
            await _db.SaveChangesAsync();

            // Tìm role mới dựa trên RoleId
            var newrole = await _db.Roles.FirstOrDefaultAsync(x => x.Id == data.RoleId);

            if (newrole == null)
            {
                return NotFound("Không có vai trò mới.");
            }

            // Xử lý với role "Student"
            if (newrole.Name == "Student")
            {
                var teacherid = await _db.Teachers.FirstOrDefaultAsync(x => x.UserId == data.Id);
                if (teacherid != null)
                {
                    _db.Teachers.Remove(teacherid);

                    var student = new Students
                    {
                        Id = Guid.NewGuid(),
                        Code = RandomCode(8),
                        UserId = data.Id
                    };

                    _db.Students.Add(student);
                    await _db.SaveChangesAsync();
                }
            }

            // Xử lý với role "Teacher"
            if (newrole.Name == "Teacher")
            {
                var studentid = await _db.Students.FirstOrDefaultAsync(x => x.UserId == data.Id);
                if (studentid != null)
                {
                    _db.Students.Remove(studentid);

                    var teacher = new Teachers
                    {
                        Id = Guid.NewGuid(),
                        Code = RandomCode(8),
                        UserId = data.Id
                    };

                    _db.Teachers.Add(teacher);
                    await _db.SaveChangesAsync();
                }
            }

            return Ok("Cập nhật thành công.");
        }

        [HttpDelete("delete-user")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound("Không có ID này trong hệ thống.");
            }
            var student = await _db.Students.FirstOrDefaultAsync(x => x.UserId == id);
            if (student != null)
            {
                var studentClasses = _db.Student_Classes.Where(x => x.StudentId == student.Id).ToList();

                foreach (var studentClass in studentClasses)
                {
                    var classEntity = await _db.Classes.FirstOrDefaultAsync(x => x.Id == studentClass.ClassId);
                    if (classEntity != null)
                    {
                        classEntity.MaxStudent -= 1;
                        _db.Classes.Update(classEntity);
                    }
                    var tests = await (from t in _db.Tests
                                       join subject in _db.Subjects on t.SubjectId equals subject.Id
                                       join subjectGrade in _db.Subject_Grades on subject.Id equals subjectGrade.SubjectId
                                       join grade in _db.Grades on subjectGrade.GradeId equals grade.Id
                                       join classEntityJoin in _db.Classes on grade.Id equals classEntityJoin.GradeId
                                       where classEntityJoin.Id == studentClass.ClassId
                                       select t).ToListAsync();

                    foreach (var test in tests)
                    {
                        test.MaxStudent -= 1;
                        _db.Tests.Update(test);
                        var testCodes = await _db.TestCodes
                            .Where(tc => tc.TestId == test.Id)
                            .OrderBy(tc => tc.Id)
                            .ToListAsync();
                        if (testCodes.Count > test.MaxStudent)
                        {
                            var excessTestCodes = testCodes.Skip(test.MaxStudent).ToList();
                            _db.TestCodes.RemoveRange(excessTestCodes);
                        }
                    }
                }
                if (studentClasses.Any())
                {
                    _db.Student_Classes.RemoveRange(studentClasses);
                }
                _db.Students.Remove(student);
            }
            var teacher = await _db.Teachers.FirstOrDefaultAsync(x => x.UserId == id);
            if (teacher != null)
            {
                _db.Teachers.Remove(teacher);
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            var publicId = user.Avartar
            .Split('/').Last().Split('.').First(); 
            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloud.DestroyAsync(deletionParams);
            return Ok("Xóa thành công");
        }
        

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModelDTO model)
        {
            var data = _db.Users.FirstOrDefault(temp => temp.UserName == model.Username);
            var student = _db.Roles.FirstOrDefault(temp => temp.Id == data.RoleId);
            var studentId = _db.Students.FirstOrDefault(temp => temp.UserId == data.Id);
            var teacherId = _db.Teachers.FirstOrDefault(temp => temp.UserId == data.Id);

            if (model.Username == data.UserName && model.Password == data.PasswordHash)
            {
                if (student.Name == "Student")
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyHere");

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                         new Claim("nameab",data.FullName.ToString()),
                         new Claim("Id",student.Name.ToString()),
                         new Claim("Idstudent",studentId.Id.ToString()),
                         new Claim("email",data.Email.ToString()),
                         new Claim("numberPhone",data.PhoneNumber.ToString()),
                         new Claim("CodeStudent", studentId.Code.ToString()),
                         
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(15),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = "https://localhost:7046/",
                        Audience = "https://localhost:7128/"
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    // Trả về token cho client
                    return Ok(new { Token = tokenString });
                }
                else
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyHere");

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                         new Claim("nameab",data.FullName.ToString()),
                         new Claim("Id",student.Name.ToString()),
                         new Claim("Idteacher",teacherId.Id.ToString()),
                         new Claim("email",data.Email.ToString()),
                         new Claim("numberPhone",data.PhoneNumber.ToString()),
                         
                         new Claim("CodeTeacher", teacherId.Code.ToString())
                            
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(15),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = "https://localhost:7046/",
                        Audience = "https://localhost:7128/"
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    // Trả về token cho client
                    return Ok(new { Token = tokenString });
                }
                // Nếu thông tin đăng nhập đúng, tạo token JWT

            }
            return Unauthorized("tên đăng nhập mật khẩu không đúng");
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {

            return Ok(new { message = "Logout successful" });
        }
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportUsersFromExcel(IFormFile file , Guid id)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File không hợp lệ.");
            }

            var users = new List<UserDTO>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return BadRequest("File Excel không chứa dữ liệu.");
                    }
                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        // Tạo ID user trước
                        var userId = Guid.NewGuid();
                        string imagePath = null;

                        var pictures = worksheet.Drawings
                            .Where(d => d.From.Row + 1 == row && d.From.Column + 1 == 3) 
                            .ToList();

                        if (pictures.Count > 0 && pictures.FirstOrDefault() is ExcelPicture excelPicture)
                        {
                            using (var imageStream = new MemoryStream(excelPicture.Image.ImageBytes))
                            {
                                var uploadimg = new ImageUploadParams
                                {
                                    File = new FileDescription(userId.ToString(), imageStream),
                                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                                };
                                var uploadResult = await _cloud.UploadAsync(uploadimg);
                                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    imagePath = uploadResult.SecureUrl.ToString();
                                }
                            }

                           
                        }

                        var user = new Users
                        {
                            Id = userId, 
                            FullName = worksheet.Cells[row, 2]?.Value?.ToString(),
                            Avartar = imagePath,
                            Email = worksheet.Cells[row, 4]?.Value?.ToString(),
                            UserName = worksheet.Cells[row, 5]?.Value?.ToString(),
                            PasswordHash = worksheet.Cells[row, 6]?.Value?.ToString(),
                            DateOfBirth = DateTime.ParseExact(worksheet.Cells[row, 7]?.Value?.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            PhoneNumber = worksheet.Cells[row, 8]?.Value?.ToString(),
                            IsLocked = true,
                            LockedEndTime = DateTime.Now,
                            CreationTime = DateTime.Now,
                            Status = 1,
                            RoleId = Guid.Parse("26948ba2-17c8-4b73-bd5b-08cca2db0f92")
                        };
                        await _db.Users.AddAsync(user);
                        await _db.SaveChangesAsync();

                        var student = new Students
                        {
                            Id = Guid.NewGuid(),
                            Code = RandomCode(8),
                            UserId = user.Id,
                        };
                        await _db.Students.AddAsync(student);
                        await _db.SaveChangesAsync();
                        var studentclass = new Student_Class
                        {
                            Id = Guid.NewGuid(),
                            JoinTime = DateTime.Now,
                            Status = 1,
                            StudentId = student.Id,
                            ClassId = id
                        };
                        await _db.Student_Classes.AddAsync(studentclass);
                        await _db.SaveChangesAsync();
                        await updateclass(id);
                    }
                }
            }
            return Ok(new { Message = "Thêm dữ liệu thành công." });
        }
        #region updeteClass

        private string RamdomCodeTestCode(int length)
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
        private async Task updateclass(Guid id)
        {
            var classUpdate = await _db.Classes.FirstOrDefaultAsync(c => c.Id == id);
            if (classUpdate != null)
            {
                classUpdate.MaxStudent = await _db.Student_Classes.Where(x => x.ClassId == id).CountAsync();
                _db.Classes.Update(classUpdate);
                await _db.SaveChangesAsync();
            }
            var testsToUpdate = await (from test in _db.Tests
                                       join subject in _db.Subjects on test.SubjectId equals subject.Id
                                       join subjectGrade in _db.Subject_Grades on subject.Id equals subjectGrade.SubjectId
                                       join grade in _db.Grades on subjectGrade.GradeId equals grade.Id
                                       join Class in _db.Classes on grade.Id equals Class.GradeId
                                       where Class.Id == id
                                       select test).ToListAsync();
            foreach (var test in testsToUpdate)
            {
                test.MaxStudent = classUpdate.MaxStudent;
                _db.Tests.Update(test);
                await _db.SaveChangesAsync();
            }
            var testClass = await (from test in _db.Tests
                                   join subj in _db.Subjects on test.SubjectId equals subj.Id
                                   join sg in _db.Subject_Grades on subj.Id equals sg.SubjectId
                                   join grade in _db.Grades on sg.GradeId equals grade.Id
                                   join cl in _db.Classes on grade.Id equals cl.GradeId
                                   where cl.Id == id
                                   select new
                                   {
                                       Test = test,
                                       MaxStudent = cl.MaxStudent,
                                       ClassId = cl.Id
                                   }).ToListAsync();

            foreach (var test in testClass)
            {
                // Đếm số lượng TestCodes hiện tại cho TestId
                var currentTestCodesCount = await _db.TestCodes.CountAsync(tc => tc.TestId == test.Test.Id);

                // Tính toán số lượng TestCodes cần thêm mới dựa trên MaxStudent
                int codesToAdd = test.MaxStudent - currentTestCodesCount;

                if (codesToAdd > 0)
                {
                    // Thêm TestCodes mới tương ứng với số lượng còn thiếu
                    var newTestCodes = Enumerable.Range(0, codesToAdd).Select(_ => new TestCodes
                    {
                        Id = Guid.NewGuid(),
                        Code = RamdomCodeTestCode(8), 
                        Status = 1,
                        TestId = test.Test.Id
                    }).ToList();

                    await _db.TestCodes.AddRangeAsync(newTestCodes);
                    await _db.SaveChangesAsync();
                }
            }
            
        }
        #endregion

        [HttpGet("export-sample")]
        public IActionResult ExportSampleExcel()
        {
            // Tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Mẫu Dữ Liệu");

                // Tạo tiêu đề cột
                var headers = new[]
                {
                    "STT", "Họ và Tên", "Hình Ảnh", "Email", "UserName", "Password", "Ngày sinh", "Số Điện Thoại"
                };

                for (int col = 1; col <= headers.Length; col++)
                {
                    worksheet.Cells[1, col].Value = headers[col - 1];
                }

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 12;
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189)); // Màu xanh đậm
                }

                // Định dạng chiều rộng cột
                worksheet.Column(1).Width = 10; 
                worksheet.Column(2).Width = 25; 
                worksheet.Column(3).Width = 25; 
                worksheet.Column(4).Width = 25; 
                worksheet.Column(5).Width = 25; 
                worksheet.Column(6).Width = 25; 
                worksheet.Column(7).Width = 25; 
                worksheet.Column(8).Width = 25; 
             

                // Dữ liệu mẫu (nếu cần)
                for (int row = 2; row <= 2; row++) // Dữ liệu mẫu từ hàng 2 đến hàng 6
                {
                    worksheet.Cells[row, 1].Value = row - 1; // STT
                    worksheet.Cells[row, 2].Value = $"Nguyễn Văn {row - 1}"; // Họ và Tên
                    worksheet.Cells[row, 3].Value = "Ảnh nhúng tại đây"; // Hình ảnh
                    worksheet.Cells[row, 4].Value = $"user{row - 1}@example.com"; // Email
                    worksheet.Cells[row, 5].Value = $"user{row - 1}"; // UserName
                    worksheet.Cells[row, 6].Value = "******"; // Password
                    worksheet.Cells[row, 7].Value = DateTime.Now.AddYears(-20).ToString("dd/MM/yyyy"); // Ngày sinh
                    worksheet.Cells[row, 8].Value = "0123456789"; // Số điện thoại
                }
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Trả file về client
                var fileName = $"Template_Sample_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(stream, contentType, fileName);
            }
        }

    }
}
