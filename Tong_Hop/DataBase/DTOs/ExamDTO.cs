using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class ExamDTO
    {
        public Guid Id { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public Guid SubjectId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid RoomId { get; set; }
        public Guid TeacherId1 { get; set; }
        public Guid TeacherId2 { get; set; }
    }
    public class GetAllExamDTO
    {
        public Guid Id { get; set; }
        
        public string? Name { get; set; }
        public int Status { get; set; }
        public string?  NameSubject { get; set; }
        public Guid idsubject { get; set; }
        public string? Nameroom { get; set; }
        public Guid idrom { get; set; }
        public string? NameTeacher1 { get; set; }
        public Guid idteacher1 { get; set; }
        public string? NameTeacher2 { get; set; }
        public Guid idteacher2 { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid IdEaxmRoom { get; set; }
    }
    public class UpdateExamDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Status { get; set; }

        public Guid idsubject { get; set; }

        public Guid idrom { get; set; }

        public Guid idteacher1 { get; set; }

        public Guid idteacher2 { get; set; }
    }

    public class GetAllExamCaThiDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int Status { get; set; }
        public string? NameSubject { get; set; }
        public Guid idsubject { get; set; }
        public string? Nameroom { get; set; }
        public Guid idrom { get; set; }
        public string? NameTeacher1 { get; set; }
        public Guid idteacher1 { get; set; }
        public string? NameTeacher2 { get; set; }
        public Guid idteacher2 { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid IdEaxmRoom { get; set; }
        public Guid IdTest { get; set; }
        public string? CodeTest { get; set; }
    }
}
