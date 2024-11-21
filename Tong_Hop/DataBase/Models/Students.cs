using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DataBase.Models
{
    public class Students
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(30, ErrorMessage = "Code không được quá 30 ký tự")]
        public string Code { get; set; }
        [ForeignKey("Id")]
        public Guid UserId { get; set; }
        public virtual Users? User { get; set; }
        public virtual ICollection<Student_Class>? Student_Class { get; set; }
        public virtual ICollection<Exam_Room_Student>? Exam_Room_Student { get; set; }
        public virtual ICollection<Learning_Summary>? Learning_Summaries { get; set; }
        public virtual ICollection<Scores>? Scores { get; set; }
        public virtual ICollection<FaceFeatures>? FaceFeatures { get; set; }
    }
}
