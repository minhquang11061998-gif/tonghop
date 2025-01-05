using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataBase.Models
{
    public class Exams
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(100, ErrorMessage = "Tên bài thi không được quá 20 ký tự")]
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        public virtual Subjects Subject { get; set; }
        public virtual ICollection<Exam_Room>? Exam_Room { get; set; }
    }
}
