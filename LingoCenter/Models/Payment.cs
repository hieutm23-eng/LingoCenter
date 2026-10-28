using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }

        [Range(1, 100000000)]
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        [Required, StringLength(120)]
        public string Reference { get; set; }

        [Required, StringLength(30), Index(IsUnique = true)]
        public string ReceiptNo { get; set; }
        public int? RecordedById { get; set; }
        public virtual User RecordedBy { get; set; }
    }
}
