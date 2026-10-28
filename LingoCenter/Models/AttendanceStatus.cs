using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }
}
