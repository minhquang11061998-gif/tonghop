using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Avartar { get; set; }
        public string? Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedEndTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastMordificationTime { get; set; }
        public int Status { get; set; }
        public Guid RoleId { get; set; }
    }
}
