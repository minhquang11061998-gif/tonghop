using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class LoginModelDTO
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class TokenResponse
    {
        public string? Token { get; set; }
    }
    public class Login_Exam_DTO
    {
        public string codelogin { get; set; }

    }
}
