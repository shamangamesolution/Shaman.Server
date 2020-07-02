using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shaman.Common.Utils.Messages;

namespace Shaman.DAL.MongoDb.FluentOperators
{
    public interface IUpdateFluent<T>
    {
        IUpdateFluent<T> Set(Expression<Func<T, object>> expression, object value);
        IUpdateFluent<T> Push<T1>(Expression<Func<T, IEnumerable<T1>>> expression, T1 value) where T1 : EntityBase;
        IUpdateFluent<T> Pull<T1>(Expression<Func<T, IEnumerable<T1>>> expression, Expression<Func<T1, bool>> filter)
            where T1 : EntityBase;
        Task Update();
    }
    
    public class UpdateFluent<T> : IUpdateFluent<T>
    {
        private readonly Expression<Func<T, bool>> _filter;
        private readonly IMongoCollection<T> _collection;
        private readonly List<UpdateDefinition<T>> _updateDefinition;
    
        public UpdateFluent(Expression<Func<T, bool>> filter, IMongoCollection<T> collection)
        {
            _filter = filter;
            _collection = collection;
            _updateDefinition = new List<UpdateDefinition<T>>();
        }
    
        public IUpdateFluent<T> Set(Expression<Func<T, object>> expression, object value)
        {
            _updateDefinition.Add(Builders<T>.Update.Set(expression, value));
            return this;
        }
    
        public async Task Update()
        {
            var combinedUpdate = Builders<T>.Update.Combine(_updateDefinition);
            await _collection.UpdateOneAsync(_filter, combinedUpdate);
        }
        
        public IUpdateFluent<T> Push<T1>(Expression<Func<T, IEnumerable<T1>>> expression, T1 value) where T1 : EntityBase
        {
            _updateDefinition.Add(Builders<T>.Update.Push<T1>(expression, value));
            return this;
        }
        
        public IUpdateFluent<T> Pull<T1>(Expression<Func<T, IEnumerable<T1>>> expression, Expression<Func<T1, bool>> filter) where T1 : EntityBase
        {
            _updateDefinition.Add(Builders<T>.Update.PullFilter<T1>(expression, filter));
            return this;
        } 
    }
}