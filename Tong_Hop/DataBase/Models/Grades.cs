using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Grades
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(2, ErrorMessage = "Tên khối không quá 2 ký tự")]
        public int Name { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Classes>? Class { get; set; }
        public virtual ICollection<Subject_Grade>? Subject_Grades { get; set; }
    }
}
