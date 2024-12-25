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
    public class Teachers
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }
        [ForeignKey("Id")]
        public Guid UserId { get; set; }
        public virtual Users? User { get; set; }
        public virtual ICollection<Classes>? Class { get; set; }
        public virtual ICollection<Exam_Room> Exam_RoomsAsTeacher1 { get; set; }
        public virtual ICollection<Exam_Room>? Exam_RoomsAsTeacher2 { get; set; }
        public virtual ICollection<Teacher_Subject>? Teacher_Subject { get; set; }
    }
}
