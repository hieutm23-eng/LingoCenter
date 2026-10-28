using System;

namespace LingoCenter.Services
{
    public static class CenterClock
    {
        public static DateTime Now
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            }
        }
    }
}
