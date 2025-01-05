using Database.DTOs;
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
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("get-all-notification")]
        public async Task<ActionResult<List<NotificationDTO>>> GetAll()
        {
            var data = await _db.Notifications.ToListAsync();

            var notificationDto = data.Select(x => new NotificationDTO
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                CreationTime = x.CreationTime,
                Status = x.Status,
                type = x.type,

            }).ToList();

            return Ok(notificationDto);
        }


        [HttpGet("get-by-id-notification")]
        public async Task<ActionResult<NotificationDTO>> GetByID(Guid Id)
        {
            var data = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == Id);

            if (data == null)
            {
                return BadRequest();
            }

            var notificationdto = new NotificationDTO
            {
                Id = data.Id,
                Title = data.Title,
                Content = data.Content,
                CreationTime = data.CreationTime,
                Status = data.Status,
                type = data.type,
            };

            return Ok(notificationdto);
        }

        [HttpPost("create-notification")]
        public async Task<IActionResult> Create(NotificationDTO notification)
        {
            try
            {
                // Tạo đối tượng Notification
                var noti = new Notifications
                {
                    Id = Guid.NewGuid(),
                    Title = notification.Title,
                    Content = notification.Content,
                    CreationTime = notification.CreationTime,
                    Status = notification.Status,
                    type = notification.type   
                };

                await _db.Notifications.AddAsync(noti);
                await _db.SaveChangesAsync();

                // Chỉ xử lý ClassIds khi type == 1 (thông báo cho một hoặc nhiều lớp)
                if (noti.type == 1)
                {
                    if (notification.ClassIds == null || !notification.ClassIds.Any())
                    {
                        return BadRequest("ClassIds field is required when sending to specific classes.");
                    }

                    var validClasses = _db.Classes
                                          .Where(c => notification.ClassIds.Contains(c.Id))
                                          .Select(c => c.Id)
                                          .ToList();

                    if (validClasses.Any())
                    {
                        var notificationClasses = validClasses.Select(classId => new Notification_Class
                        {
                            Id = Guid.NewGuid(),
                            NotificationId = noti.Id,
                            ClassId = classId,
                            Status = 1
                        }).ToList();

                        _db.Notification_Classes.AddRange(notificationClasses);
                        await _db.SaveChangesAsync();
                    }
                }

                return Ok("Thêm thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [HttpPut("update-notification")]
        public async Task<IActionResult> Update(NotificationDTO notificationDTO)
        {
            var data = await _db.Notifications
                .Include(n => n.Notification_Classe)
                .FirstOrDefaultAsync(x => x.Id == notificationDTO.Id);

            if (data != null)
            {
                // Cập nhật các trường thông báo nhưng không thay đổi thông tin lớp
                data.Title = notificationDTO.Title;
                data.Content = notificationDTO.Content;
                data.CreationTime = notificationDTO.CreationTime;
                data.Status = notificationDTO.Status;

                // Không cập nhật danh sách lớp, chỉ trả về danh sách lớp hiện tại nếu có
                var classIds = data.Notification_Classe.Select(nc => nc.ClassId).ToList();

                _db.Notifications.Update(data);
                await _db.SaveChangesAsync();

                // Trả về danh sách các lớp liên quan để hiển thị trên giao diện nếu cần
                return Ok(new
                {
                    message = "Cập nhật thành công",
                    classIds = classIds // Trả về danh sách lớp liên quan
                });
            }
            return BadRequest("Đã xảy ra lỗi");
        }

        [HttpDelete("delete-notification")]
        public async Task<IActionResult> Delete(Guid Id)
        {
            var data = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == Id);
            if (data != null)
            {
                _db.Notifications.Remove(data);
                await _db.SaveChangesAsync();
                return Ok("Xóa thành công");
            }
            return BadRequest("Lỗi");
        }

        [HttpGet("detail-notification")]
        public async Task<ActionResult<NotificationDTO>> Detail(Guid Id)
        {
            var notification = await _db.Notifications
             .Include(n => n.Notification_Classe) // Giả sử bạn có bảng NotificationClasses để lưu các lớp liên quan
             .ThenInclude(nc => nc.Class)
             .FirstOrDefaultAsync(n => n.Id == Id);

            if (notification == null)
            {
                return NotFound();
            }

            var dto = new NotificationDTO
            {
                Id = notification.Id,
                Title = notification.Title,
                Content = notification.Content,
                CreationTime = notification.CreationTime,
                Status = notification.Status,
                ClassIds = notification.Notification_Classe.Select(nc => nc.ClassId).ToList(),
                ClassNames = notification.Notification_Classe.Select(nc => nc.Class.Name).ToList()
            };

            return Ok(dto);

        }

        [HttpGet("get-notifications-student")]
        public async Task<ActionResult<List<NotificationDTO>>> GetClassNotifications(Guid studentId)
        {
            try
            {
                // Lấy thông báo cho học sinh qua liên kết các bảng
                var notifications = await (from sc in _db.Student_Classes
                                           join nc in _db.Notification_Classes on sc.ClassId equals nc.ClassId
                                           join n in _db.Notifications on nc.NotificationId equals n.Id
                                           where sc.StudentId == studentId && n.type == 1
                                           select new NotificationDTO
                                           {
                                               Id = n.Id,
                                               Title = n.Title,
                                               Content = n.Content,
                                               CreationTime = n.CreationTime,
                                               Status = n.Status,
                                               type = n.type,
                                               ClassIds = n.Notification_Classe.Select(nc => nc.ClassId).ToList(),
                                               ClassNames = n.Notification_Classe.Select(nc => nc.Class.Name).ToList()
                                           }).ToListAsync();

                if (!notifications.Any())
                {
                    return Ok("No notifications found for this student.");
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving notifications: {ex.Message}");
            }
        }
    }
}
