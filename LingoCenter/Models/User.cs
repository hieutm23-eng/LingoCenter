using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; }

        [Required, EmailAddress, StringLength(150), Index(IsUnique = true)]
        public string Email { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedLogins { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
