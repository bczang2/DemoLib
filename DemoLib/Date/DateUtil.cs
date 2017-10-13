using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLib.Date
{
    public static class DateUtil
    {
        private static DateTime _stime = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        /// 转化为日期
        /// </summary>
        /// <param name="inputVal"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string inputVal, DateTime defaultVal)
        {
            DateTime result = defaultVal;
            if (!string.IsNullOrWhiteSpace(inputVal))
            {
                if (!DateTime.TryParse(inputVal, out result))
                {
                    result = defaultVal;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取本周第一天
        /// </summary>
        /// <param name="inputVal"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfWeek(this DateTime inputVal)
        {
            int curWeek = Convert.ToInt32(inputVal.DayOfWeek);
            curWeek = curWeek == 0 ? 6 : curWeek - 1;

            return inputVal.AddDays(-curWeek).Date;
        }

        /// <summary>
        /// 获取本周最后一天
        /// </summary>
        /// <param name="inputVal"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfWeek(this DateTime inputVal)
        {
            int daydiff = inputVal.DayOfWeek - DayOfWeek.Sunday;
            daydiff = daydiff != 0 ? 7 - daydiff : daydiff;

            return inputVal.AddDays(daydiff).Date;
        }

        /// <summary>
        /// 转化为时间戳
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime datetime)
        {
            return (long)datetime.ToUniversalTime().Subtract(_stime).TotalMilliseconds;
        }

        /// <summary>
        /// 时间戳转化为日期
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromTimeStamp(this long timestamp)
        {
            return _stime.AddMilliseconds(timestamp).ToLocalTime();
        }
    }
}
