using System;

namespace Shaman.Common.Utils.Extensions
{
    public static class RandomExtensions
    {
        public static float GetNextFloat(this Random random, float minValue, float maxValue)
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }
    }
}
