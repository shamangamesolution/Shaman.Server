using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Shaman.DAL.MongoDb.FluentOperators
{
    public interface IGetFluent<T>
    {
        IGetFluent<T> Include(Expression<Func<T, object>> expression);
        Task<T> GetOne();
        Task<List<T>> GetAll();
    }
    
    public class GetFluent<T> : IGetFluent<T>
    {
        private readonly Expression<Func<T, bool>> _filter;
        private readonly IMongoCollection<T> _collection;
        private readonly List<ProjectionDefinition<T>> _projectionDefinitions;
        
        public GetFluent(Expression<Func<T, bool>> filter, IMongoCollection<T> collection)
        {
            _filter = filter;
            _collection = collection;
            _projectionDefinitions = new List<ProjectionDefinition<T>>();
        }
    
        public IGetFluent<T> Include(Expression<Func<T, object>> expression)
        {
            _projectionDefinitions.Add(Builders<T>.Projection.Include(expression));
            return this;
        }
    
        public async Task<T> GetOne()
        {
            FindOptions<T> options = new FindOptions<T> { Projection = Builders<T>.Projection.Combine(_projectionDefinitions)};
            var cursor = await _collection.FindAsync(_filter, options);
            return await cursor.SingleOrDefaultAsync();
        }
        
        public async Task<List<T>> GetAll()
        {
            FindOptions<T> options = new FindOptions<T> { Projection = Builders<T>.Projection.Combine(_projectionDefinitions)};
            var cursor = await _collection.FindAsync(_filter, options);
            return await cursor.ToListAsync();
        }
    }
}