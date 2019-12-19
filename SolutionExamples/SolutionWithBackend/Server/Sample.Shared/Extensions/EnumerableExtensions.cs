using System.Collections.Generic;

namespace Sample.Shared.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool TryFirstOrDefault<T>(this IEnumerable<T> source, out T value)
        {
            value = default(T);
            using (var iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    value = iterator.Current;
                    return true;
                }
                return false;
            }
        }
    }
}