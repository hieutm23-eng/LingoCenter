using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LingoCenter.Models
{
    public static class DomainExtensions
    {
        public static int CountActive(this IEnumerable<Enrollment> values)
        {
            int n = 0;
            foreach (var e in values)
                if (e.Status != EnrollmentStatus.Withdrawn)
                    n++;
            return n;
        }
    }
}
