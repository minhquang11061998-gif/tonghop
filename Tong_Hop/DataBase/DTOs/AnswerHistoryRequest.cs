using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.DTOs
{
    public class AnswerHistoryRequest
    {
        public string CodeTest { get; set; }
        public string StudentId { get; set; }
        public List<Guid> AnswerIds { get; set; }
    }
}
