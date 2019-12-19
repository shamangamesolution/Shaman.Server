using System.Collections.Generic;
using System.Linq;

namespace Sample.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        
        public static bool IsEqual<TKey, TValue>(this Dictionary<TKey, TValue> dict, Dictionary<TKey, TValue> other)
        {
            // early-exit checks
            if (other == null)
                return dict == null;
            
            if (dict == null)
                return false;
            
            if (object.ReferenceEquals(dict, other))
                return true;
            if (dict.Count != other.Count)
                return false;

            // check keys are the same
            foreach (var k in dict.Keys)
                if (!other.ContainsKey(k))
                    return false;

            // check values are the same
            foreach (var k in dict.Keys)
                if (!dict[k].Equals(other[k]))
                    return false;

            return true;
        }
        
        public static bool IsEqual(this Dictionary<byte, List<byte>> dict, Dictionary<byte, List<byte>> other)
        {
            // early-exit checks
            if (other == null)
                return dict == null;
            
            if (dict == null)
                return false;
            
            if (object.ReferenceEquals(dict, other))
                return true;
            if (dict.Count != other.Count)
                return false;

            // check keys are the same
            foreach (var k in dict.Keys)
                if (!other.ContainsKey(k))
                    return false;

            // check values are the same
            foreach (var k in dict.Keys)
            {
                if (other[k] == null && dict[k] != null)
                    return false;
                
                if (other[k] != null && dict[k] == null)
                    return false;

                if (dict[k] == null && other[k] == null)
                    continue;
                
                if (dict[k].Count() != other[k].Count())
                    return false;

                for (var i = 0; i < dict[k].Count(); i++)
                {
                    if (!dict[k][i].Equals(other[k][i]))
                        return false;
                }
            }

            return true;
        }
    }
}