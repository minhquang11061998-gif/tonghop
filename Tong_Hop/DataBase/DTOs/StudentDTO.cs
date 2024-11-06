using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTOs
{
    public class StudentDTO
    {
        public Guid Id { get; set; }
        public Guid examRoomTestCodeId { get; set; }

        [StringLength(30, ErrorMessage = "Code không được quá 30 ký tự")]
        public string Code { get; set; }
        public string Name { get; set; }
        public string SubjectName { get; set; }
        public int GradeName { get; set; }
        public string RoomName { get; set; }
        public string ClassName { get; set; }
        public string ExamName { get; set; }
        public string TestCode { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
