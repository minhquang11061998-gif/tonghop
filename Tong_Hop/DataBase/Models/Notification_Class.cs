using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Notification_Class
    {
        [Key]
        public Guid Id { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid NotificationId { get; set; }
        [ForeignKey("Id")]
        public Guid ClassId { get; set; }
        public virtual Notifications? Notification { get; set; }
        public virtual Classes? Class { get; set; }
    }
}
