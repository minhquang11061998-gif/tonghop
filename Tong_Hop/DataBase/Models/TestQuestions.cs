using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DataBase.Models
{
    public class TestQuestions
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength]
        public string QuestionName { get; set; }
        public int Type { get; set; }
        public int Level { get; set; }
        [MaxLength]
        public string RightAnswer { get; set; }
        public string CreatedByName { get; set; }
        [ForeignKey("Id")]
        public Guid? TestId { get; set; }
        public virtual Tests? Tests { get; set; }
        public virtual ICollection<TestQuestionAnswers>? TestQuestionAnswer { get; set; }
        public virtual ICollection<TestCode_TestQuestion>? TestCode_TestQuestions { get; set; }
    }
}
