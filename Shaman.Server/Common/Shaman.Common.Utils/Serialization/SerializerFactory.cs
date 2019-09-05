using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Logging;

namespace Shaman.Common.Utils.Serialization
{

    
    public class SerializerFactory : ISerializerFactory
    {
        private IShamanLogger _logger;
        private List<ISerializer> _packers = new List<ISerializer>();
        private string _source;

        public SerializerFactory(IShamanLogger logger)
        {
            _logger = logger;
        }
        
        public void InitializeDefaultSerializers(int minLen, string source)
        {
            _source = source;
        }
        
        public ISerializer GetSimpleSerializer()
        {
            throw new NotImplementedException();
        }
        public ISerializer GetStrictSerializer()
        {
            throw new NotImplementedException();
        }
        
        public ISerializer GetBinarySerializer()
        { 
            var binarySerializer = new BinaryWriterSerializer(_logger);            
            return binarySerializer;   
        }
    }
}