using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbBulkWriter<T>
    {
        Task<long> Write();
        void Add(WriteModel<T> writeModel);
    }
    
    public class MongoDbBulkWriter<T> : IMongoDbBulkWriter<T>
    {
        private readonly IMongoCollection<T> _collection;
        private List<WriteModel<T>> _models = new List<WriteModel<T>>();
        
        public MongoDbBulkWriter(IMongoCollection<T> collection)
        {
            _collection = collection;
        }
        
        public void Add(WriteModel<T> writeModel)
        {
            _models.Add(writeModel);
        }

        public async Task<long> Write()
        {
            var result = await _collection.BulkWriteAsync(_models);
            return result.ModifiedCount + result.InsertedCount + result.DeletedCount;
        }
    }
}