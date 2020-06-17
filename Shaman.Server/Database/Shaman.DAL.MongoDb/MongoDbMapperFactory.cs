using Shaman.Common.Utils.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbMapperFactory
    {
        IMongoDbMapper GetMapper<T>() where T : EntityBase;
    }

    public class DefaultMongoDbMapperFactory : IMongoDbMapperFactory
    {
        public IMongoDbMapper GetMapper<T>() where T : EntityBase
        {
            return new DefaultMapper<T>();
        }
    }
}