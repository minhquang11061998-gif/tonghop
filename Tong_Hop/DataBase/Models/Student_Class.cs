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
    public class Student_Class
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime JoinTime { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid ClassId { get; set; }
        [ForeignKey("Id")]
        public Guid StudentId { get; set; }
        public virtual Classes? Class { get; set; }
        public virtual Students? Student { get; set; }
    }
}
