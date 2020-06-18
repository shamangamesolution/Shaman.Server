using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shaman.Common.Utils.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbConnector
    {
        void Connect();
        Task<List<T>> GetAll<T>() where T : EntityBase;
        Task<T> Get<T>(int id) where T : EntityBase;
        Task<List<T>> Get<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task Remove<T>(int id) where T : EntityBase;
        Task Remove<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task RemoveAll<T>() where T : EntityBase;
        Task Create<T>(T record) where T : EntityBase;
        Task Update<T>(int id, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase; 
        Task Update<T>(Expression<Func<T, bool>> filter, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase;

        IUpdateFluent<T> UpdateWhere<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
    }
    
    public class MongoDbConnector : IMongoDbConnector
    {
        private readonly ICustomMongoClientSettings _clientSettings;
        private readonly IMongoDbMapperFactory _mapperFactory;
        
        private IMongoClient _mongoClient;
        private IMongoDatabase _database;
        
        public MongoDbConnector(ICustomMongoClientSettings clientSettings, IMongoDbMapperFactory mapperFactory)
        {
            _clientSettings = clientSettings;
            _mapperFactory = mapperFactory;
        }

        public void Connect()
        {
            _mongoClient = new MongoClient(_clientSettings.GetSettings());
        }

        IMongoCollection<T> GetCollection<T>() where T : EntityBase
        {
            _database = _mongoClient.GetDatabase(_clientSettings.GetDataBaseName());
            var mapper = _mapperFactory.GetMapper<T>();
            return _database.GetCollection<T>(mapper.GetCollectionName());
        }

        public async Task<List<T>> GetAll<T>() where T : EntityBase
        {
            var collection = GetCollection<T>();
            var result = await collection.FindAsync(_ => true);
            return await result.ToListAsync();
        }

        public async Task<T> Get<T>(int id) where T : EntityBase
        {
            var collection = GetCollection<T>();
            var cursor = await collection.FindAsync(record => record.Id == id);
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task Remove<T>(int id) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.DeleteOneAsync(record => record.Id == id);
        }

        public async Task Remove<T>(Expression<Func<T, bool>> filter) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.DeleteManyAsync(filter);
        }

        public async Task RemoveAll<T>() where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.DeleteManyAsync(_ => true);
        }

        public async Task<List<T>> Get<T>(Expression<Func<T, bool>> filter) where T : EntityBase
        {
            var collection = GetCollection<T>();
            var cursor = await collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }

        public async Task Create<T>(T record) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.InsertOneAsync(record);
        }

        public async Task Update<T>(int id, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase 
        {
            await Update(record => record.Id == id, fieldProvider);
        }

        public async Task Update<T>(Expression<Func<T, bool>> filter, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase
        {
            var collection = GetCollection<T>();
            var updateDefinition = new List<UpdateDefinition<T>>();
            foreach (var dataField in fieldProvider.Get())
            {
                updateDefinition.Add(Builders<T>.Update.Set(dataField.Expression, dataField.Value));
            }
            var combinedUpdate = Builders<T>.Update.Combine(updateDefinition);
            await collection.UpdateOneAsync(filter, combinedUpdate);
        }
        
        
        public IUpdateFluent<T> UpdateWhere<T>(Expression<Func<T, bool>> filter) where T : EntityBase
        {
            var collection = GetCollection<T>();
            var updateDefinition = new List<UpdateDefinition<T>>();
            return new UpdateFluent<T>(filter, collection,updateDefinition);
        }


    }

    public interface IUpdateFluent<T>
    {
        IUpdateFluent<T> Set(Expression<Func<T, object>> expression, object value);
        Task Update();
    }

    public class UpdateFluent<T> : IUpdateFluent<T>
    {
        private readonly Expression<Func<T, bool>> _filter;
        private readonly IMongoCollection<T> _collection;
        private readonly List<UpdateDefinition<T>> _updateDefinition;

        public UpdateFluent(Expression<Func<T, bool>> filter, IMongoCollection<T> collection,
            List<UpdateDefinition<T>> updateDefinition)
        {
            _filter = filter;
            _collection = collection;
            _updateDefinition = updateDefinition;
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
    }

}