using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using Shaman.Common.Utils.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbRepository
    {
        void Connect();
        Task<List<T>> GetAll<T>() where T : EntityBase;
        Task<T> Get<T>(int id) where T : EntityBase;
        Task<List<T>> Get<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task Remove<T>(int id) where T : EntityBase;
        Task Remove<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task RemoveAll<T>() where T : EntityBase;
        Task Create<T>(T record) where T : EntityBase;
        Task Update<T>(int id, object updateDefinition) where T: EntityBase;
        Task Update<T>(Expression<Func<T, bool>> filter, object updateDefinition) where T: EntityBase;

    }
    
    public class MongoDbRepository : IMongoDbRepository
    {
        private readonly ICustomMongoClientSettings _clientSettings;
        private readonly IMongoDbMapperFactory _mapperFactory;
        
        private IMongoClient _mongoClient;
        private IMongoDatabase _database;
        
        public MongoDbRepository(ICustomMongoClientSettings clientSettings, IMongoDbMapperFactory mapperFactory)
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

        public async Task Update<T>(int id, object updateDefinition) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.UpdateOneAsync(record => record.Id == id, new ObjectUpdateDefinition<T>(updateDefinition));
        }

        public async Task Update<T>(Expression<Func<T, bool>> filter, object updateDefinition) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.UpdateManyAsync(filter, new ObjectUpdateDefinition<T>(updateDefinition));
        }
    }
}