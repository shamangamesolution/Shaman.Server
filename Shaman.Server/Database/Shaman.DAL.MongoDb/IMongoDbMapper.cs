using Shaman.Common.Utils.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbMapper
    {
        string GetCollectionName();
    }

    public class DefaultMapper<T> : IMongoDbMapper where T:EntityBase    
    {
        public DefaultMapper()
        {
            //TODO insert here different mapping stuff kinda following
            
            // BsonClassMap.RegisterClassMap<T>(map =>
            // {
            //     map.AutoMap();
            //     map.SetIgnoreExtraElements(true);
            //     map.MapIdMember(x => x.Id);
            // });
        }
        
        public string GetCollectionName()
        {
            return typeof(T).Name;
        }
    }
}