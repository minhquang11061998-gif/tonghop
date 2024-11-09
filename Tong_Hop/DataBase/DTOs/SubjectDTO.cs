using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class SubjectDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public List<Guid>? GradeIds { get; set; }
        public List<PointTypeDto>? PointTypeIds { get; set; } // Thêm danh sách PointTypeId
        public Guid TeacherId { get; set; }

    }

    public class PointTypeDto
    {
        public Guid IdPointType { get; set; }
        public int Quantity { get; set; } // Giá trị Quantity tuỳ thuộc vào từng PointType
    }
}
