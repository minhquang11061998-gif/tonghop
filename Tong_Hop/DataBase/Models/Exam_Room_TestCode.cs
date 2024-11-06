using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Exam_Room_TestCode
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public Guid TestId { get; set; }
        [ForeignKey("Id")]
        public Guid ExamRoomId { get; set; }
        public virtual Tests? Tests { get; set; }
        public virtual Exam_Room? Exam_Room { get; set; }
        public virtual ICollection<Exam_Room_Student>? Exam_Room_Students { get; set; }
    }
}
