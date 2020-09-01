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
        private readonly FilterDefinition<T> _filterDefinition;
        
        public GetFluent(Expression<Func<T, bool>> filter, IMongoCollection<T> collection)
        {
            _filter = filter;
            _collection = collection;
            _projectionDefinitions = new List<ProjectionDefinition<T>>();
            _filterDefinition = null;
        }

        public GetFluent(FilterDefinition<T> filter, IMongoCollection<T> collection)
        {
            _filter = null;
            _collection = collection;
            _projectionDefinitions = new List<ProjectionDefinition<T>>();
            _filterDefinition = filter;
        }

        public IGetFluent<T> Include(Expression<Func<T, object>> expression)
        {
            _projectionDefinitions.Add(Builders<T>.Projection.Include(expression));
            return this;
        }
    
        public async Task<T> GetOne()
        {
            FindOptions<T> options = new FindOptions<T> { Projection = Builders<T>.Projection.Combine(_projectionDefinitions)};
            if (_filter != null)
            {
                var cursor = await _collection.FindAsync(_filter, options);
                return await cursor.SingleOrDefaultAsync();
            }

            if (_filterDefinition != null)
            {
                var cursor = await _collection.FindAsync(_filterDefinition, options);
                return await cursor.SingleOrDefaultAsync();
            }
            
            throw new Exception($"GetOne error: both _filter and _filterDefinition are not defined");
        }
        
        public async Task<List<T>> GetAll()
        {
            FindOptions<T> options = new FindOptions<T> { Projection = Builders<T>.Projection.Combine(_projectionDefinitions)};
            if (_filter != null)
            {
                var cursor = await _collection.FindAsync(_filter, options);
                return await cursor.ToListAsync();
            }
            
            if (_filterDefinition != null)
            {
                var cursor = await _collection.FindAsync(_filterDefinition, options);
                return await cursor.ToListAsync();
            }
            
            throw new Exception($"GetOne error: both _filter and _filterDefinition are not defined");
        }
    }
}