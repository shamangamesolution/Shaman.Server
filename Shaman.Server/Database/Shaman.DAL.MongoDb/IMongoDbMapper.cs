using System.Linq;
using MongoDB.Bson.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbMapper
    {
        string GetCollectionName();
    }

    public class DefaultMapper<T> : IMongoDbMapper where T:EntityBase    
    {
        public string GetCollectionName()
        {
            return typeof(T).Name;
        }
    }
}