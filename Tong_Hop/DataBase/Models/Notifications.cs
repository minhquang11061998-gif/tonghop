using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Notifications
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(256, ErrorMessage = "Title không quá 256 ký tự ")]
        public string Title { get; set; }

        [MaxLength]
        public string Content { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int type { get; set; }
        public virtual ICollection<Notification_Class>? Notification_Classe { get; set; }
    }
}
