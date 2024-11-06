using DataBase.Data;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Xml.Linq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceIdController : ControllerBase
    {
        private readonly AppDbContext _db;
        private  VideoCapture _capture;
        private  CascadeClassifier _faceCascade;
        private  LBPHFaceRecognizer _faceRecognizer;
        private static bool _isCapturing = false;

        public FaceIdController(AppDbContext db)
        {
            _db = db;
            _faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

            // Mở camera (ID 0 - camera mặc định)
            _capture = new VideoCapture(0);
            if (!_capture.IsOpened)
            {
                throw new System.Exception("Unable to open camera.");
            }

            // Tạo một recognizer (để huấn luyện nhận diện khuôn mặt)
            _faceRecognizer = new LBPHFaceRecognizer();
        }
        [HttpPost("register-face")]
        public IActionResult RegisterFace(string userId)
        {
            try
            {
                // Capture frame từ camera
                using var frame = _capture.QueryFrame().ToImage<Bgr, byte>();
                var faces = _faceCascade.DetectMultiScale(frame, 1.1, 10, new Size(50, 50));

                if (faces.Length == 0)
                    return BadRequest("No face detected.");

                // Chỉ xử lý khuôn mặt đầu tiên
                var face = faces[0];
                var grayFace = frame.Convert<Gray, byte>().GetSubRect(face).Resize(200, 200, Emgu.CV.CvEnum.Inter.Cubic);
                var faceFeatures = ExtractFaceFeatures(grayFace);

                // Lưu đặc trưng khuôn mặt vào XML
                SaveFaceFeaturesToXml(userId, faceFeatures);

                // Train và ghi model nếu cần
                _faceRecognizer.Train(new[] { grayFace.Mat }, new[] { 1 });

                return Ok("Đữ liệu khuôn mặt đã được lưu thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        private static double[] ExtractFaceFeatures(Image<Gray, byte> faceImage)
        {

            return faceImage.Bytes.Select(b => (double)b).ToArray();
        }

        // Hàm lưu đặc trưng vào file XML
        private void SaveFaceFeaturesToXml(string userId, double[] features)
        {
            string xmlFilePath = "face_features.xml";
            XDocument xmlDoc;

            // Load XML file hoặc tạo mới nếu chưa tồn tại
            if (System.IO.File.Exists(xmlFilePath))
            {
                xmlDoc = XDocument.Load(xmlFilePath);
            }
            else
            {
                xmlDoc = new XDocument(new XElement("Faces"));
            }

            string featuresString = string.Join(",", features);

            // Kiểm tra xem UserId đã tồn tại hay chưa
            var existingFace = xmlDoc.Root.Elements("Face")
                .FirstOrDefault(f => f.Element("UserId")?.ToString() == userId);

            if (existingFace != null)
            {
                // Cập nhật đặc trưng nếu đã tồn tại
                existingFace.Element("Features").Value = featuresString;
            }
            else
            {
                // Thêm mới nếu chưa tồn tại
                var newFaceElement = new XElement("Face",
                    new XElement("UserId", userId),
                    new XElement("Features", featuresString)
                );
                xmlDoc.Root.Add(newFaceElement);
            }

            // Save XML file
            xmlDoc.Save(xmlFilePath);
        }
        private static byte[] ConvertFrameToJpeg(Image<Bgr, byte> frame)
        {
            using (var ms = new MemoryStream())
            {
                frame.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
        [HttpGet("camera-stream")]
        public async Task CameraStream()
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";

            using (var capture = new VideoCapture(0))
            {
                var faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

                while (true)
                {
                    using (var frame = capture.QueryFrame().ToImage<Bgr, byte>())
                    {
                        if (frame == null)
                            continue;

                        // Nhận diện khuôn mặt
                        var faces = faceCascade.DetectMultiScale(frame, 1.1, 4, new Size(30, 30));

                        // Chỉ nhận diện khuôn mặt đầu tiên
                        if (faces.Length > 0)
                        {
                            var face = faces[0];
                            // Vẽ hình chữ nhật xung quanh khuôn mặt đầu tiên
                            frame.Draw(face, new Bgr(Color.Red), 2);
                        }

                        var jpegData = ConvertFrameToJpeg(frame);

                        await Response.Body.WriteAsync(
                            Encoding.UTF8.GetBytes($"--frame\r\nContent-Type: image/jpeg\r\n\r\n"));
                        await Response.Body.WriteAsync(jpegData, 0, jpegData.Length);
                        await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("\r\n"));
                    }

                    await Task.Delay(67); // Đợi để đạt 30 FPS
                }
            }
        }
        [HttpPost("check-face")]
        public IActionResult CheckFace()
        {
            try
            {
                // Capture frame từ camera
                using var frame = _capture.QueryFrame().ToImage<Bgr, byte>();
                var faces = _faceCascade.DetectMultiScale(frame, 1.1, 10, new Size(50, 50));

                if (faces.Length == 0)
                    return BadRequest("No face detected.");

                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyHere");
                var tokenHandler = new JwtSecurityTokenHandler();

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
                var student = claimsPrincipal.Claims.First(claim => claim.Type == "Idstudent").Value;
                foreach (var face in faces)
                {
                    var grayFace = frame.Convert<Gray, byte>().GetSubRect(face).Resize(200, 200, Emgu.CV.CvEnum.Inter.Cubic);
                    var faceFeatures = ExtractFaceFeatures(grayFace);

                    // Kiểm tra khuôn mặt với dữ liệu đã lưu
                    var result = CheckFaceAgainstStoredData(faceFeatures);

                    if (result != null && student == result.Value.UserId)
                    {

                        // Gọi hàm reset camera
                        return Ok(new { Message = "Face matched.", UserId = result.Value.UserId });

                    }
                }

                return Ok("No matching face found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        
        private (string UserId, double Similarity)? CheckFaceAgainstStoredData(double[] newFaceFeatures)
        {
            string xmlFilePath = "face_features.xml";

            if (!System.IO.File.Exists(xmlFilePath))
                return null;

            var xmlDoc = XDocument.Load(xmlFilePath);
            double maxSimilarity = 0.0;
            string matchedUserId = null;
            double similarityThreshold = 0.95; // Ngưỡng để xác định độ trùng khớp

            foreach (var faceElement in xmlDoc.Root.Elements("Face"))
            {
                var userId = (string)faceElement.Element("UserId");
                var featuresString = (string)faceElement.Element("Features");
                var storedFeatures = featuresString.Split(',').Select(double.Parse).ToArray();

                double similarity = CalculateSimilarity(newFaceFeatures, storedFeatures);

                if (similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    matchedUserId = userId;
                }
            }

            // Chỉ trả về kết quả nếu độ tương đồng đạt yêu cầu
            if (maxSimilarity >= similarityThreshold)
            {
                return (matchedUserId, maxSimilarity);
            }

            return null; // Trả về null nếu không có khuôn mặt nào khớp
        }


        private double CalculateSimilarity(double[] features1, double[] features2)
        {
            if (features1.Length != features2.Length || features1.Length == 0)
                return 0.0;

            double sumProduct = 0.0;
            double sumSquare1 = 0.0;
            double sumSquare2 = 0.0;

            for (int i = 0; i < features1.Length; i++)
            {
                sumProduct += features1[i] * features2[i];
                sumSquare1 += features1[i] * features1[i];
                sumSquare2 += features2[i] * features2[i];
            }

            double denominator = Math.Sqrt(sumSquare1) * Math.Sqrt(sumSquare2);
            return denominator == 0 ? 0.0 : sumProduct / denominator;
        }
        [HttpDelete("remove-face")]
        public IActionResult RemoveFaceFeatures(string userId)
        {
            try
            {
                string xmlFilePath = "face_features.xml";

                // Kiểm tra nếu tệp XML tồn tại
                if (!System.IO.File.Exists(xmlFilePath))
                {
                    return NotFound("XML file not found.");
                }

                // Load XML file
                var xmlDoc = XDocument.Load(xmlFilePath);

                // Tìm phần tử `Face` theo `userId`
                var faceElement = xmlDoc.Root.Elements("Face")
                    .FirstOrDefault(f => (string)f.Element("UserId") == userId);

                if (faceElement != null)
                {
                    // Xóa phần tử nếu tìm thấy
                    faceElement.Remove();

                    // Lưu lại XML sau khi xóa
                    xmlDoc.Save(xmlFilePath);

                    return Ok($"Face features for userId '{userId}' have been removed.");
                }
                else
                {
                    return NotFound($"No face features found for userId '{userId}'.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
        [HttpGet("recognize-face")]
        public IActionResult RecognizeFace()
        {
            // Tải mô hình đã lưu
            if (System.IO.File.Exists("face_features.xml"))
            {
                _faceRecognizer.Read("face_features.xml");
            }
            else
            {
                return StatusCode(500, "Model not trained or saved yet.");
            }

            using var frame = _capture.QueryFrame().ToImage<Bgr, byte>();
            var faces = _faceCascade.DetectMultiScale(frame, 1.1, 10, new Size(50, 50));

            if (faces.Length == 0)
                return BadRequest("No face detected.");

            foreach (var face in faces)
            {
                var grayFace = frame.Convert<Gray, byte>().GetSubRect(face).Resize(200, 200, Emgu.CV.CvEnum.Inter.Cubic);
                var result = _faceRecognizer.Predict(grayFace);

                // Kiểm tra nếu khuôn mặt trùng khớp
                if (result.Label == 1 && result.Distance < 100)
                {
                    return Ok("Face recognized.");
                }
            }

            return Unauthorized("Face not recognized.");
        }
        public class Login_Exam_DTO
        {
            public int codelogin { get; set; }

        }

        [HttpPost("Login-exam")]
        public IActionResult GetLogin(Login_Exam_DTO login)
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
                    return Ok("thành công ");
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

