using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }
        public CourseLevel Level { get; set; }

        [Required, StringLength(2000)]
        public string Description { get; set; }

        [Range(1, 104)]
        public int DurationWeeks { get; set; }

        [Range(1, 100000000)]
        public decimal TuitionFee { get; set; }

        [Required, StringLength(4000)]
        public string Syllabus { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<CenterClass> Classes { get; set; } = new List<CenterClass>();
    }
}
