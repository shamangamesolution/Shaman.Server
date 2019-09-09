using System;
using System.Globalization;

namespace Shaman.Messages.Extensions
{
    public static class FloatExtensions
    {
        public static float GetFloat(object obj)
        {
            //checking  separators
            string sample = "10.11";
            string separator = string.Empty;
            float result = Convert.ToSingle(sample, CultureInfo.InvariantCulture);
            if (result < 100)
                separator = ".";
            else
                separator = ",";
            return Convert.ToSingle(obj.ToString().Replace(",", separator).Replace(".", separator), CultureInfo.InvariantCulture);
        }
        public static float GetFromPercent(this float val)
        {
            return (100 + val) / 100;
        }
    }
}
