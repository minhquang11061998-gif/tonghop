﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Models
{
    public class Semesters
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(30, ErrorMessage = "Tên kỳ không quá 30 ký tự")]
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public virtual ICollection<Learning_Summary>? Learning_Summarys { get; set; }
    }
}
