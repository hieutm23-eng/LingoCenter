using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Index("UX_Attendance", 1, IsUnique = true)]
        public int ClassSessionId { get; set; }
        public virtual ClassSession ClassSession { get; set; }

        [Index("UX_Attendance", 2, IsUnique = true)]
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
