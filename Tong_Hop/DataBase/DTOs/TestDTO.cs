using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class TestDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Minute { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int Maxstudent { get; set; }
        public Guid SubjectId { get; set; }
        public Guid PointTypeId { get; set; }
        public Guid ClassId { get; set; }
        public Guid ExamRoomId { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public Guid RoomId { get; set; }
		public Guid ExamId { get; set; }
		public Guid TeacherId1 { get; set; }
		public Guid TeacherId2 { get; set; }
	}


}
