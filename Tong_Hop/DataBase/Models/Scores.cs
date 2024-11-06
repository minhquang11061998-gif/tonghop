using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataBase.Models
{
    public class Scores
    {
        [Key]
        public Guid Id { get; set; }
        public double Score { get; set; }
        [ForeignKey("Id")]
        public Guid StudentId { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        [ForeignKey("Id")]
        public Guid PointTypeId { get; set; }
        public virtual Students? Students { get; set; }
        public virtual Subjects? Subjects { get; set; }
        public virtual PointTypes? PointTypes { get; set; }
    }
}
