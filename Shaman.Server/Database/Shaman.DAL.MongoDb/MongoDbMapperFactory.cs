using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.DAL.MongoDb
{
    public interface IMongoDbMapperFactory
    {
        IMongoDbMapper GetMapper<T>() where T : EntityBase;
    }

    public class DefaultMongoDbMapperFactory : IMongoDbMapperFactory
    {
        private object _mutex = new object();
        private Dictionary<Type, IMongoDbMapper> _mappers = new Dictionary<Type, IMongoDbMapper>();
        
        public DefaultMongoDbMapperFactory()
        {
            //remove Id field from mapping to prevent mongo _id field mapping to it
            if (!BsonClassMap.IsClassMapRegistered(typeof(EntityBase)))
            {
                BsonClassMap.RegisterClassMap<EntityBase>(cm =>
                {
                    //unmap all fields
                    cm.SetIgnoreExtraElements(true);
                    cm.SetIgnoreExtraElementsIsInherited(true);
                    
                    //map id separately - it is not registered by _id by default
                    cm.MapField(m => m.Id);
                });
            }
        }
        
        public virtual IMongoDbMapper GetMapper<T>() where T : EntityBase
        {
            lock (_mutex)
            {
                if (!_mappers.TryGetValue(typeof(T), out var mapper))
                {
                    //automap all derived entities
                    if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
                    {
                        BsonClassMap.RegisterClassMap<T>(cm => { cm.AutoMap(); });
                    }

                    mapper = new DefaultMapper<T>();
                    _mappers.Add(typeof(T), mapper);
                }

                return mapper;
            }
        }
    }
}