using DataBase.Data;
using DataBase.DTOs;
using DataBase.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceIdController : ControllerBase
    {
        private readonly LBPHFaceRecognizer _faceRecognizer;
        private readonly CascadeClassifier _faceCascade;
        private readonly AppDbContext _db;

        public FaceIdController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _faceRecognizer = new LBPHFaceRecognizer();
            _faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
        }
        [HttpPost("process")]
        public IActionResult ProcessImage([FromBody] ImageRequest request)
        {
            try
            {
                // Chuyển base64 thành byte array
                byte[] imageBytes = Convert.FromBase64String(request.Image);
                using var ms = new MemoryStream(imageBytes);
               using var image = new Bitmap(ms);

                // Xử lý bằng Emgu CV
                using var emguImage =  BitmapExtension.ToImage<Bgr, byte>(image);
                var grayImage = emguImage.Convert<Gray, byte>();

                // Phát hiện khuôn mặt
                var faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");
                var faces = faceCascade.DetectMultiScale(grayImage, 1.1, 10);

                // Trả về kết quả (ví dụ: số khuôn mặt phát hiện)
                return Ok(new { FaceCount = faces.Length });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        public class ImageRequest
        {
            public string Image { get; set; } // Dữ liệu base64 từ client
        }
        [HttpPost("register-face")]
        public IActionResult RegisterFace(Guid userId, FaceDTO dTO)
        {
            try
            {
            
                byte[] imageBytes = Convert.FromBase64String(dTO.img);
                using var ms = new MemoryStream(imageBytes);
                using var bitmap= new Bitmap(ms);
                // Chuyển mảng byte thành ảnh sử dụng Emgu.CV
                using var frame = BitmapExtension.ToImage<Bgr, byte>(bitmap);

                // Phát hiện khuôn mặt trong ảnh
                var faces = _faceCascade.DetectMultiScale(frame, 1.1, 10, new Size(50, 50));

                if (faces.Length == 0)
                    return BadRequest("No face detected.");

                // Chỉ xử lý khuôn mặt đầu tiên
                var face = faces[0];
                var grayFace = frame.Convert<Gray, byte>().GetSubRect(face).Resize(200, 200, Emgu.CV.CvEnum.Inter.Cubic);

                // Trích xuất đặc trưng khuôn mặt
                var faceFeatures = ExtractFaceFeatures(grayFace);

                // Chuyển đổi vector đặc trưng thành mảng byte để lưu
                byte[] faceFeatureBytes = ConvertFaceFeaturesToByteArray(faceFeatures);

                var data = new FaceFeatures
                {
                    Guid = Guid.NewGuid(),
                    StudentID = userId,
                    img = faceFeatureBytes
                };

                // Lưu vào cơ sở dữ liệu (ví dụ như Entity Framework)
                _db.FaceFeatures.Add(data);
                _db.SaveChanges();

                // Huấn luyện mô hình nhận diện khuôn mặt
                _faceRecognizer.Train(new[] { grayFace.Mat }, new[] { 1 });

                return Ok("Face data has been saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private byte[] ConvertFaceFeaturesToByteArray(double[] faceFeatures)
        {
            // Chuyển đổi vector đặc trưng thành mảng byte
            return faceFeatures.Select(f => (byte)(f * 255)).ToArray(); // Giả sử giá trị nằm trong khoảng [0, 1]
        }


        private static double[] ExtractFaceFeatures(Image<Gray, byte> faceImage)
        {

            return faceImage.Bytes.Select(b => (double)b).ToArray();
        }

        
        [HttpPost("recognize")]
        public IActionResult Recognize([FromBody] string imagePath)
        {
            try
            {
                // Đọc ảnh từ đường dẫn và chuyển sang grayscale
                var img = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Grayscale);

                if (img.IsEmpty)
                {
                    return BadRequest("Ảnh không tồn tại hoặc không thể đọc.");
                }

                // Phát hiện khuôn mặt
                var faces = _faceCascade.DetectMultiScale(img, 1.1, 10, Size.Empty);
                if (faces.Length == 0)
                {
                    return NotFound("Không phát hiện được khuôn mặt.");
                }

                foreach (var face in faces)
                {
                    // Cắt và resize khuôn mặt
                    var faceImage = new Mat(img, face);
                    CvInvoke.Resize(faceImage, faceImage, new Size(100, 100), 0, 0, Emgu.CV.CvEnum.Inter.Cubic);

                    // Dự đoán khuôn mặt
                    var result = _faceRecognizer.Predict(faceImage);
                    int label = result.Label;
                    double distance = result.Distance;

                    // Kiểm tra khoảng cách dự đoán, tùy chỉnh ngưỡng để cải thiện độ chính xác
                    if (distance < 0.8)
                    {
                        return Ok(new { Label = label, Distance = distance });
                    }
                }

                return NotFound("Khuôn mặt không được nhận diện.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Login-exam")]
        public IActionResult GetLogin([FromBody] Login_Exam_DTO login)
        {
            // Lấy token từ tiêu đề Authorization
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Key bí mật để xác thực token
            var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyHere");

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Giải mã token
                var claimsPrincipal = tokenHandler.ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    },
                    out SecurityToken validatedToken);

                // Lấy IdStudent từ claim trong token
                var studentIdFromToken = claimsPrincipal.Claims.First(claim => claim.Type == "Idstudent").Value;

                // Kiểm tra mã code và IdStudent từ cơ sở dữ liệu
                var code = _db.Tests.FirstOrDefault(x => x.Code == login.codelogin);
                var student = _db.Students.FirstOrDefault(x => x.Id.ToString() == studentIdFromToken);

                if (code != null && student != null)
                {
                    var examRoomTestCodeId = (from exam in _db.Exam_Room_TestCodes
                                              join test in _db.Tests
                                              on exam.TestId equals test.Id
                                              where test.Code == login.codelogin
                                              select exam.Id).FirstOrDefault();

                    if (examRoomTestCodeId != Guid.Empty)
                    {
                        var newExamRoomStudent = new Exam_Room_Student_DTO
                        {
                            Id = Guid.NewGuid(),
                            ChenkTime = DateTime.Now,
                            Status = 1,
                            ExamRoomTestCodeId = examRoomTestCodeId,
                            StudentId = Guid.Parse(studentIdFromToken)
                        };

                        var examRoomStudentEntity = new Exam_Room_Student
                        {
                            Id = newExamRoomStudent.Id,
                            ChenkTime = newExamRoomStudent.ChenkTime,
                            Status = newExamRoomStudent.Status,
                            ExamRoomTestCodeId = newExamRoomStudent.ExamRoomTestCodeId,
                            StudentId = newExamRoomStudent.StudentId
                        };

                        _db.Exam_Room_Students.Add(examRoomStudentEntity);
                        _db.SaveChanges();
                        return Ok("Thành công");
                    }
                }
            }
            catch (Exception)
            {
                return Unauthorized("Token không hợp lệ hoặc hết hạn");
            }

            return Unauthorized("Code hoặc ID sai");
        }

    }         
        
}
