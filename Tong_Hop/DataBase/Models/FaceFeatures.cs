using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class FaceFeatures
    {
        [Key]
        public Guid Guid { get; set; }
        public byte[] img { get; set; }
        public Guid StudentID { get; set; }
       
    }
}
