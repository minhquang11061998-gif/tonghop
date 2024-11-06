using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Rooms
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(50, ErrorMessage = "Name ko quá 30 ký tự")]
        public string Name { get; set; }

        [StringLength(30, ErrorMessage = "Code ko quá 30 ký tự")]
        public string Code { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Exam_Room> Exam_Room { get; set; }
    }
}
