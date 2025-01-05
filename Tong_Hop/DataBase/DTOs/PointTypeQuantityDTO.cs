using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTOs
{
    public class PointTypeQuantityDTO
    {
        public string SubjectName { get; set; }
        public string PointTypeName { get; set; }
        public int Quantity { get; set; }
        public string Scores { get; set; }

        // Các danh sách điểm cho từng loại điểm
        public List<double> FifteenMinutes { get; set; }
        public List<double> Miệng { get; set; }
        public List<double> FortyFiveMinutes { get; set; }
        public List<double> MidTerm { get; set; }
        public List<double> Final { get; set; }
    }
}
