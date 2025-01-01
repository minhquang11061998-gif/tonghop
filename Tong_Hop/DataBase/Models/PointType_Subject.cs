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
    public class PointType_Subject
    {
        [Key]
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("Id")]
        public Guid SubjectId { get; set; }
        [ForeignKey("Id")]
        public Guid PointTypeId { get; set; }
        public virtual Subjects? Subject { get; set; }
        public virtual PointTypes? PointType { get; set; }
    }
}
