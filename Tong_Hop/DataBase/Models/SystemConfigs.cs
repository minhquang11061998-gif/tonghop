using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class SystemConfigs
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(100, ErrorMessage = "Tên thông báo không được quá 100 ký tự")]
        public string Name { get; set; }

        [StringLength(254, ErrorMessage = "Email không được quá 256 ký tự")]
        [EmailAddress(ErrorMessage = "Email ko đúng định dạng")]
        public string Email { get; set; }

        [StringLength(12, ErrorMessage = "Số điện không được quá 12 ký tự")]
        public string PhoneNumber { get; set; }
        [MaxLength]
        public string? address { get; set; }
        public int Type { get; set; }
        public bool IsViewed { get; set; } = false;

        [MaxLength]
        public string Value { get; set; }
    }
}
