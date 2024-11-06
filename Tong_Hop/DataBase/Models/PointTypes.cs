using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace DataBase.Models
{
    public class PointTypes
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Tests>? tests { get; set; }
        public virtual ICollection<Learning_Summary>? Learning_Summaries { get; set; }
        public virtual ICollection<PointType_Subject>? PointType_Subjects { get; set; }
        public virtual ICollection<Scores>? Scores { get; set; }
    }
}
