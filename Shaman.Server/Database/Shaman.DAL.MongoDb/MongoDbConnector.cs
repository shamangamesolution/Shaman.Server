using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Shaman.DAL.MongoDb.FluentOperators;
using Shaman.Serialization.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbConnector
    {
        void Connect();
        Task<List<T>> GetAll<T>() where T : EntityBase;
        Task<T> Get<T>(string id) where T : EntityBase;
        IGetFluent<T> GetFields<T>(string id) where T : EntityBase;
        IGetFluent<T> GetFields<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task<List<T>> Get<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task Remove<T>(string id) where T : EntityBase;
        Task Remove<T>(Expression<Func<T, bool>> filter) where T : EntityBase;
        Task RemoveAll<T>() where T : EntityBase;
        Task Create<T>(T record) where T : EntityBase;
        Task Update<T>(string id, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase; 
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
            var settings = _clientSettings.GetSettings();
            var connectionString = _clientSettings.GetConnectionString();
            if (settings != null)
                _mongoClient = new MongoClient(settings);
            else if (!string.IsNullOrWhiteSpace(connectionString))
                _mongoClient = new MongoClient(connectionString);
            else
                throw new Exception($"Mongo configuration error: no connection string or settings defined");
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

        private FilterDefinition<T> GetIdFilter<T>(string id)
        {
            return Builders<T>.Filter.Eq("_id", new ObjectId(id));
        }
        
        public async Task<T> Get<T>(string id) where T : EntityBase
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception($"Get<T> error: id is null");
            var collection = GetCollection<T>();
            var filter = GetIdFilter<T>(id);
            var entity = await (await collection.FindAsync(filter)).SingleOrDefaultAsync();
            return entity;
        }

        public IGetFluent<T> GetFields<T>(string id) where T : EntityBase
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception($"GetFields<T> error: id is null");
            var collection = GetCollection<T>();
            return new GetFluent<T>(GetIdFilter<T>(id), collection);
        }

        public IGetFluent<T> GetFields<T>(Expression<Func<T, bool>> filter) where T : EntityBase
        {
            var collection = GetCollection<T>();
            return new GetFluent<T>(filter, collection);
        }

        public async Task Remove<T>(string id) where T : EntityBase
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception($"Remove<T> error: id is null");
            var collection = GetCollection<T>();
            await collection.DeleteOneAsync(GetIdFilter<T>(id));
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

        private UpdateDefinition<T> GetUpdateDefinition<T>(IMongoDbFieldProvider<T> fieldProvider)
        {
            var updateDefinition = new List<UpdateDefinition<T>>();
            foreach (var dataField in fieldProvider.Get())
            {
                updateDefinition.Add(Builders<T>.Update.Set(dataField.Expression, dataField.Value));
            }
            return Builders<T>.Update.Combine(updateDefinition);
        }
        
        public async Task Update<T>(string id, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new Exception($"Update<T> error: id is null");
            var collection = GetCollection<T>();
            await collection.UpdateOneAsync(GetIdFilter<T>(id), GetUpdateDefinition<T>(fieldProvider));
        }

        public async Task Update<T>(Expression<Func<T, bool>> filter, IMongoDbFieldProvider<T> fieldProvider) where T : EntityBase
        {
            var collection = GetCollection<T>();
            await collection.UpdateOneAsync(filter, GetUpdateDefinition<T>(fieldProvider));
        }
        
        
        public IUpdateFluent<T> UpdateWhere<T>(Expression<Func<T, bool>> filter) where T : EntityBase
        {
            var collection = GetCollection<T>();
            return new UpdateFluent<T>(filter, collection);
        }
    }
}