using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required, StringLength(400)]
        public string Message { get; set; }

        [Required, StringLength(100), Index("UX_Notice", 2, IsUnique = true)]
        public string EventKey { get; set; }

        [Index("UX_Notice", 1, IsUnique = true)]
        public int RecipientKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
