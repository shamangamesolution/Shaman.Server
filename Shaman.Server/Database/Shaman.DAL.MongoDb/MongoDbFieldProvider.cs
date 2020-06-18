using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shaman.DAL.MongoDb
{
    public class MongoDbFieldInfo<T>
    {
        public MongoDbFieldInfo(Expression<Func<T, object>> expression, object value)
        {
            Expression = expression;
            Value = value;
        }

        public Expression<Func<T,object>> Expression { get; set; }
        public object Value { get; set; }
        
    }
    public interface IMongoDbFieldProvider<T>
    {
        void Add(Expression<Func<T,object>> expression, object value);
        IEnumerable<MongoDbFieldInfo<T>> Get();
    }
    
    public class MongoDbFieldProvider<T> : IMongoDbFieldProvider<T>
    {
        private readonly List<MongoDbFieldInfo<T>> _list = new List<MongoDbFieldInfo<T>>();
        
        public void Add(Expression<Func<T,object>> expression, object value)
        {
            _list.Add(new MongoDbFieldInfo<T>(expression, value));
        }

        public IEnumerable<MongoDbFieldInfo<T>> Get()
        {
            return _list;
        }
    }
}