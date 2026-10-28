using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class ClassSession
    {
        public int Id { get; set; }
        public int CenterClassId { get; set; }
        public virtual CenterClass CenterClass { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }

        [Required, StringLength(200)]
        public string Topic { get; set; }
    }
}
