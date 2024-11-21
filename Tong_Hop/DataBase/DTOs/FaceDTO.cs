using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class FaceDTO
    {
        public Guid Guid { get; set; }
        public string img { get; set; }
        public Guid StudentID { get; set; }
    }
}
