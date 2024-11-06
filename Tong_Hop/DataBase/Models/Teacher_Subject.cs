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
    public class Teacher_Subject
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public Guid TeacherId { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        public virtual Teachers? Teacher { get; set; }
        public virtual Subjects? Subject { get; set; }
    }
}
