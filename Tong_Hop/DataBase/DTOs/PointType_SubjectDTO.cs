using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class PointType_SubjectDTO
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public Guid PointTypeId { get; set; }
        public int Quantity { get; set; }
    }
}
