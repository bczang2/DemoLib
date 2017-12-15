using System;

namespace DemoLib.Util
{
    public static class DateTimeUtil
    {
        private static DateTime _starttime = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long ToTimeStamp(this DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(_starttime).TotalMilliseconds;
        }

        public static DateTime FromTimeStamp(this long timestamp)
        {
            return _starttime.AddMilliseconds(timestamp).ToLocalTime();
        }
    }
}
