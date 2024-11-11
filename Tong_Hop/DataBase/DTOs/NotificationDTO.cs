using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs
{
    public class NotificationDTO
    {
        public Guid Id { get; set; }

        [StringLength(256, ErrorMessage = "Title không quá 256 ký tự ")]
        public string? Title { get; set; }

        [MaxLength]
        public string? Content { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int type { get; set; }
        public List<Guid>? ClassIds { get; set; }
        // Danh sách tên các lớp liên quan (chỉ để hiển thị)
        public List<string> ClassNames { get; set; } = new List<string>();
    }
    
    
}
