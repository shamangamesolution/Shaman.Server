using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Shaman.Serialization.Messages;

namespace Shaman.DAL.MongoDb.Tests
{
    public class TestMongoDbMapperFactory : DefaultMongoDbMapperFactory
    {
        public TestMongoDbMapperFactory()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TestEntity)))
            {
                BsonClassMap.RegisterClassMap<TestEntity>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.StringId)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }
        }
    }
}