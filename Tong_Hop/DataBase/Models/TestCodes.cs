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
    public class TestCodes
    {
        [Key]
        public Guid Id { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        [ForeignKey("Id")]
        public Guid TestId { get; set; }
        public virtual Tests? Tests { get; set; }
        public virtual ICollection<TestCode_TestQuestion>? TestCode_TestQuestions { get; set; }
    }
}
