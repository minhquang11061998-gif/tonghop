using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string Name { get; set; }
        public int Status { get; set; }
        public virtual ICollection<Users> Users { get; set; }
    }
}
