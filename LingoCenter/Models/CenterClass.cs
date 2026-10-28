using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class CenterClass
    {
        public int Id { get; set; }

        [Required, StringLength(30), Index(IsUnique = true)]
        public string Code { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int TeacherId { get; set; }
        public virtual User Teacher { get; set; }

        [Required, StringLength(40)]
        public string Room { get; set; }

        [Range(1, 100)]
        public int Capacity { get; set; }
        public ClassStatus Status { get; set; }
        public virtual ICollection<ClassSession> Sessions { get; set; } = new List<ClassSession>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        [NotMapped]
        public int Occupied
        {
            get
            {
                return Enrollments.CountActive();
            }
        }

        [NotMapped]
        public string Availability
        {
            get
            {
                return Status == ClassStatus.Closed ? "Closed" : Occupied >= Capacity ? "Full" : "Open";
            }
        }
    }
}
