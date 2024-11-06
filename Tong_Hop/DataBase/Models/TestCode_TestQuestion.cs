using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class TestCode_TestQuestion
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("Id")]
        public Guid TestCodeId { get; set; }
        [ForeignKey("Id")]
        public Guid TestQuestionId { get; set; }
        public virtual TestCodes? TestCodes { get; set; }
        public virtual TestQuestions? TestQuestion { get; set; }
    }
}
