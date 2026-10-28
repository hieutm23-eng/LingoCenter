using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LingoCenter.Models
{
    public class LoginVm
    {
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), StringLength(100)]
        public string Password { get; set; }
    }

    public class RegisterVm : LoginVm
    {
        [Required, StringLength(120)]
        public string FullName { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class ProfileVm
    {
        [Required, StringLength(120)]
        public string FullName { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }
    }

    public class PasswordVm
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, StringLength(100, MinimumLength = 10)]
        public string NewPassword { get; set; }

        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class ScoreRow
    {
        public int EnrollmentId { get; set; }

        [Range(0, 10)]
        public decimal? Midterm { get; set; }

        [Range(0, 10)]
        public decimal? Final { get; set; }
    }

    public class AttendanceRow
    {
        public int EnrollmentId { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class DashboardVm
    {
        public int Students { get; set; }
        public int Classes { get; set; }
        public int Enrollments { get; set; }
        public decimal Revenue { get; set; }
        public decimal Outstanding { get; set; }
        public List<CenterClass> ClassList { get; set; }
        public List<Payment> RecentPayments { get; set; }
        public List<Metric> RevenueMonths { get; set; }
        public List<Metric> Popularity { get; set; }
        public List<Metric> Workloads { get; set; }
    }

    public class Metric
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    public class TimetableVm
    {
        public DateTime Week { get; set; }
        public List<ClassSession> Sessions { get; set; }
    }
}
