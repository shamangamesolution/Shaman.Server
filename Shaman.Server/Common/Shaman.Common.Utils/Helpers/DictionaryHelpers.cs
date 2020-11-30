using System.Collections.Generic;

namespace Shaman.Common.Utils.Helpers
{
    public class DictionaryHelpers
    {
        public static bool AreDictionariesEqual(Dictionary<byte, object> dict1, Dictionary<byte, object> dict2)
        {
            if (dict1 == null && dict2 == null)
                return true;
            if (dict1 == null)
                return false;
            if (dict2 == null)
                return false;

            if (dict1.Count != dict2.Count)
                return false;

            foreach (var item in dict1)
            {
                if (!dict2.ContainsKey(item.Key) || !Equals(dict2[item.Key], item.Value))
                    return false;
            }

            return true;
        }
    }
}