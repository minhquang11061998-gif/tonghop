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

        [StringLength(50, ErrorMessage = "Tên phòng không quá 50 ký tự")]
        public string Name { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Exam_Room> Exam_Room { get; set; }
    }
}
