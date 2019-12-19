using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Messages;

namespace Sample.Shared.Extensions
{
    public static class EntityDictionaryExtensions
    {
        public static bool IsNullOrEmpty<T>(this EntityDictionary<T> dictionary) where T:EntityBase, new()
        {
            return dictionary == null || !dictionary.Any();
        }
    }
}