using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class Exam_Room_Histories_DTO
    {
        public Guid Id { get; set; }
      
        public Guid ExamRoomStudentId { get; set; }
     
        public Guid TestQuestionAnswerId { get; set; }
    }
}
