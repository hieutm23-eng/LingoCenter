using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Index("UX_StudentClass", 1, IsUnique = true)]
        public int StudentId { get; set; }
        public virtual User Student { get; set; }

        [Index("UX_StudentClass", 2, IsUnique = true)]
        public int CenterClassId { get; set; }
        public virtual CenterClass CenterClass { get; set; }
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AgreedFee { get; set; }

        [Range(0, 10)]
        public decimal? Midterm { get; set; }

        [Range(0, 10)]
        public decimal? Final { get; set; }
        public bool ScoresPublished { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [NotMapped]
        public decimal Paid
        {
            get
            {
                decimal n = 0;
                foreach (var p in Payments)
                    if (p.Status == PaymentStatus.Paid)
                        n += p.Amount;
                return n;
            }
        }

        [NotMapped]
        public decimal Balance
        {
            get
            {
                return AgreedFee - Paid;
            }
        }

        [NotMapped]
        public decimal? Result
        {
            get
            {
                return Midterm.HasValue && Final.HasValue ? Math.Round(Midterm.Value * 0.4m + Final.Value * 0.6m, 2) : (decimal? )null;
            }
        }
    }
}
