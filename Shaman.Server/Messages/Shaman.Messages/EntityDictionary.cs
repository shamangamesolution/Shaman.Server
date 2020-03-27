using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Shaman.Common.Utils.Messages;

namespace Shaman.Messages
{
    public class EntityDictionary<T> : IEnumerable<T> where T:EntityBase
    {
        private readonly ConcurrentDictionary<long, T> _dict;
        
        public EntityDictionary()
        {
            _dict = new ConcurrentDictionary<long, T>();
        }

        public EntityDictionary(IEnumerable<T> list)
        {
            _dict = new ConcurrentDictionary<long, T>();
            foreach (var item in list)
                _dict.TryAdd(item.Id, item);
        }
        
        public EntityDictionary(Dictionary<long, T> dict)
        {
            _dict = new ConcurrentDictionary<long, T>(dict);
        }
        
        public void Add(T item)
        {
            _dict.TryAdd(item.Id, item);
        }

        public int Count => _dict.Count;

        /// <summary>
        /// Search entity by ID
        /// </summary>
        /// <param name="id">Id value of entity to search</param>
        public T this[long id]
        {
            get
            {
                if (_dict.TryGetValue(id, out var item))
                    return item;
                
                return null;
            }
        }

        public bool ContainsKey(long id)
        {
            return _dict.ContainsKey(id);
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

        public void Remove(long key)
        {
            _dict.TryRemove(key, out var item);
        }

        public void Clear()
        {
            _dict.Clear();
        }
        
    }
}