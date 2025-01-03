﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(100, ErrorMessage = "Tên người dùng không được quá 100 ký tự")]
        public string? FullName { get; set; }
        public string? Avartar { get; set; }

        [StringLength(256, ErrorMessage = "Email không được quá 256 ký tự")]
        [EmailAddress(ErrorMessage = "Email ko đúng định dạng")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Tên đăng nhập không được quá 50 ký tự")]
        [RegularExpression(@"\S+", ErrorMessage = "Tên đăng nhập chứa ít nhất 1 ký tự không phải khoảng trắng")]
        public string UserName { get; set; }

        [StringLength(256, ErrorMessage = "Mất khẩu không được quá 256 ký tự")]
        public string PasswordHash { get; set; }
        public DateTime? DateOfBirth { get; set; }

        [StringLength(12, ErrorMessage = "Số điện thoại không được quá 12 ký tự")]
        public string? PhoneNumber { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedEndTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastMordificationTime { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid RoleId { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<Students>? Student { get; set; }
        public virtual ICollection<Teachers>? Teacher { get; set; }
    }
}
