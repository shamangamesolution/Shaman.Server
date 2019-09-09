using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;

namespace Shaman.Messages
{
    public class EntityDictionary<T> : IEnumerable<T> where T:EntityBase
    {
        private ConcurrentDictionary<int, T> _dict;
        
        public EntityDictionary()
        {
            _dict = new ConcurrentDictionary<int, T>();
        }

        public void Add(T item)
        {
            _dict.TryAdd(item.Id, item);
        }
        
        public T this[int id]
        {
            get
            {
                if (!_dict.ContainsKey(id))
                    return null;
                return _dict[id];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(var val in _dict)
            {
                yield return val.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}