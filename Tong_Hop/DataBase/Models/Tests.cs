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
    public class Tests
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(30, ErrorMessage = "Tên bài kiểm tả không quá 30 ký tự")]
        public string Name { get; set; }
        public int Code { get; set; }
        public int? Minute { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int MaxStudent { get; set; }
        [ForeignKey("Id")]
        public Guid ClassId { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        [ForeignKey("Id")]
        public Guid PointTypeId { get; set; }
        public virtual Subjects? Subject { get; set; }
        public virtual Classes? Classes { get; set; }
        public virtual PointTypes? PointType { get; set; }
        public virtual ICollection<TestQuestions>? testQuestions { get; set; }
        public virtual ICollection<TestCodes>? testCodes { get; set; }
    }
}
