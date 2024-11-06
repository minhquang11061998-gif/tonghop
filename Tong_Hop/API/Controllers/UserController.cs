using Database.DTOs;
using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
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
                var data = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

                if (data == null)
                {
                    return NotFound("Danh sach trong");
                }

                var UserDTO = new UserDTO
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

                };

                return Ok(UserDTO);
            }
            catch (Exception)
            {
                return BadRequest("loi");
            }
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> Create([FromForm] UserDTO user, IFormFile avatarFile)
        {
            try
            {
                var userId = Guid.NewGuid();
                string avatarPath = null;

                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Đường dẫn thư mục lưu trữ ảnh
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Avatars");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Đặt tên file là userId + phần mở rộng của file gốc
                    var fileName = userId.ToString() + Path.GetExtension(avatarFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file ảnh vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn ảnh để lưu vào database
                    avatarPath = "/avatars/" + fileName;
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
                        var student = new Students
                        {
                            Id = Guid.NewGuid(),
                            Code = RandomCode(8),
                            UserId = data.Id
                        };
                        await _db.Students.AddAsync(student);
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
                    }
                    await _db.SaveChangesAsync();
                }

                return Ok("Them thanh cong");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> Update(UserDTO userDTO)
        {
            var data = await _db.Users.FirstOrDefaultAsync(x => x.Id == userDTO.Id);

            if (data == null)
            {
                return NotFound("Khong tim thay user");
            }

            data.FullName = userDTO.FullName;
            data.Avartar = userDTO.Avartar;
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

            _db.Users.Update(data);
            _db.SaveChanges();

            // Tìm role mới dựa trên rollID
            var newrole = await _db.Roles.FirstOrDefaultAsync(x => x.Id == data.RoleId);

            if (newrole == null)
            {
                return NotFound("Khong co role khac");
            }
            if (newrole.Name == "Student")
            {
                // Kiểm tra và thêm Student nếu chưa tồn tại
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
                    _db.SaveChanges();
                }
            }

            if (newrole.Name == "Teacher")
            {
                // Kiểm tra và thêm Teacher nếu chưa tồn tại
                var studentid = await _db.Students.FirstOrDefaultAsync(x => x.UserId == data.Id);
                if (studentid != null)
                {
                    _db.Students.Remove(studentid);

                    var teachr = new Teachers
                    {
                        Id = Guid.NewGuid(),
                        Code = RandomCode(8),
                        UserId = data.Id
                    };

                    _db.Teachers.Add(teachr);
                    _db.SaveChanges();
                }
            }

            return Ok("Update thành công");
        }

        [HttpDelete("delete-user")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (data ==null)
            {
                return NotFound("Khong co id nay");
            }

            var student = await _db.Students.FirstOrDefaultAsync(x => x.Id == id);
            if (student != null)
            {
                _db.Students.Remove(student);
            }

            // Xóa thông tin của user trong bảng teachers (nếu có)
            var teacher = await _db.Teachers.FirstOrDefaultAsync(x => x.Id == id);
            if (teacher != null)
            {
                _db.Teachers.Remove(teacher);
            }

            // Xóa user trong bảng users
            _db.Users.Remove(data);
            await _db.SaveChangesAsync();

            return Ok("Xóa thành công");
        }


        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(Guid Id, IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return BadRequest("Ảnh không hợp lệ");
            }

            // Tìm user theo ID
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại");
            }

            // Lưu ảnh vào thư mục và cập nhật đường dẫn
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(fileStream); // Lưu file vào thư mục
            }

            // Lưu đường dẫn ảnh vào cơ sở dữ liệu
            user.Avartar = "/avatars/" + fileName;

            // Cập nhật thời gian thay đổi cuối cùng
            user.LastMordificationTime = DateTime.Now;

            // Lưu thay đổi vào db
            await _db.SaveChangesAsync();

            return Ok("Tải ảnh thành công");
        }

        public class TestCodeRequest
        {
            public string TestCode { get; set; }
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
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
                         new Claim("email",data.Email.ToString()),
                         new Claim("numberPhone",data.PhoneNumber.ToString()),
                         new Claim("CodeStudent", studentId.Code.ToString()),
                         //new Claim("CodeTeacher", teacherId.Code.ToString())
                            //new Claim("Id", student != null ? student.Name : "N/A"),
                            //new Claim("Idteacher",teacher != null? teacher.Code:"N/A")
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
                         new Claim("email",data.Email.ToString()),
                         new Claim("numberPhone",data.PhoneNumber.ToString()),

                         //new Claim("CodeStudent", studentId.Code.ToString()),
                         new Claim("CodeTeacher", teacherId.Code.ToString())
                            //new Claim("Id", student != null ? student.Name : "N/A"),
                            //new Claim("Idteacher",teacher != null? teacher.Code:"N/A")
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
    }
}
