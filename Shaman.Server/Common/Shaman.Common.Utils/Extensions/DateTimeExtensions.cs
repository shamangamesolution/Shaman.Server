using System;

namespace Shaman.Common.Utils.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsYesterdayOrMore(this DateTime dt)
        {
            return IsYesterday(dt)
                   || (DateTime.UtcNow - dt).TotalDays >= 1;
        }

        public static bool IsYesterday(this DateTime dt)
        {
            DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
            if (dt >= yesterday && dt < DateTime.UtcNow.Date)
                return true;
            return false;
        }
    }
}