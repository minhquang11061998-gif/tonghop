using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.DTOs
{
    public class ListQuestionDTO
    {
        public Guid id { get; set; }
        public string code { get; set; }
        public int type { get; set; }

        public string name { get; set; }
        public string nametest { get; set; }
        public int namegrade { get; set; }
        public int totalquestion { get; set; }
        public string usermane { get; set; }
        public Guid idnew { get; set; }


    }
    public class listdetailquestion
    {
        public Guid Id { get; set; }
        public string Questionname { get; set; }
        public int level {  get; set; }
        public int Type { get; set; }
        public string RightAnswer { get; set; }
        public List<AnswerDTO> answer { get; set; }
    }
}
