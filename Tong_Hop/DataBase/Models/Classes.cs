using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Classes
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }
        [StringLength(20, ErrorMessage = "Tên không được quá 20 ký tự")]
        public string Name { get; set; }
        public int Status { get; set; }
        public int MaxStudent { get; set; }
        [ForeignKey("Id")]
        public Guid TeacherId { get; set; }
        [ForeignKey("Id")]
        public Guid GradeId { get; set; }
        public virtual Teachers? Teacher { get; set; }
        public virtual Grades? Grade { get; set; }
        public virtual ICollection<Notification_Class>? Notification_Classe { get; set; }
        public virtual ICollection<Student_Class>? Student_Classes { get; set; }
    }
}
