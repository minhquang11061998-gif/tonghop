using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Exam_Room
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid RoomId { get; set; }
        [ForeignKey("Id")]
        public Guid ExamId { get; set; }
        [ForeignKey("Id")]
        public Guid TeacherId1 { get; set; }
        [ForeignKey("Id")]
        public Guid TeacherId2 { get; set; }
        public virtual Rooms? Room { get; set; }
        public virtual Exams? Exam { get; set; }

        [ForeignKey("TeacherId1")]
        [InverseProperty("Exam_RoomsAsTeacher1")]
        public virtual Teachers? Teacher1 { get; set; }

        [ForeignKey("TeacherId2")]
        [InverseProperty("Exam_RoomsAsTeacher2")]
        public virtual Teachers? Teacher2 { get; set; }
        public virtual ICollection<Exam_Room_TestCode> Exam_Room_TestCode { get; set; }
    }
}
