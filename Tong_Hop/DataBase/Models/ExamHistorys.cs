using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class ExamHistorys
    {
        [Key]
        public Guid Id { get; set; }
        public double Score { get; set; }

        [MaxLength]
        //public string? Note { get; set; }
        public DateTime CreationTime { get; set; }
        [ForeignKey("Id")]
        public Guid ExamRoomStudentId { get; set; }
        public virtual Exam_Room_Student? Exam_Room_Student { get; set; }
    }
}
