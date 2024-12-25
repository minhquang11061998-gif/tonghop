using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace DataBase.Models
{
    public class Subjects
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(100, ErrorMessage = "Name ko quá 100 ký tự")]
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Subject_Grade>? Subject_Grade { get; set; }
        public virtual ICollection<Tests>? Test { get; set; }
        public virtual ICollection<Exams>? Exam { get; set; }
        public virtual ICollection<Teacher_Subject>? Teacher_Subject { get; set; }
        public virtual ICollection<Scores>? Scores { get; set; }
        public virtual ICollection<Learning_Summary>? Learning_Summaries { get; set; }
        public virtual ICollection<PointType_Subject>? PointType_Subjects { get; set; }
    }
}
