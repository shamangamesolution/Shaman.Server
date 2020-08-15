using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.TestTools.ClientPeers
{
    //todo remove
    
    public static class MessageFactory
    {
        private static readonly Lazy<Dictionary<ushort, Type>> TypesMap = new Lazy<Dictionary<ushort, Type>>(CreateMessageMap);

        static Dictionary<ushort, Type> CreateMessageMap()
        {
            var messageBaseType = typeof(PingRequest);
            
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                t.GetConstructor(Array.Empty<Type>()) != null).ToList();

            var instances = messageTypes.Select(Activator.CreateInstance).OfType<MessageBase>();

            var result = new Dictionary<ushort, Type>();
            foreach(var item in instances)
                if (!result.ContainsKey(item.OperationCode))
                    result.Add(item.OperationCode, item.GetType());
            
            return result;
        }

        public static MessageBase DeserializeMessageForTest(ushort operationCode, byte[] byteArray, int offset,
            int length)
        {
            var key = operationCode;
            var instance = (MessageBase) Activator.CreateInstance(TypesMap.Value[key]);

            using (var reader = new BinaryReader(new MemoryStream(byteArray, offset, length)))
            {
                var deserializer = new BinaryTypeReader(reader);
                instance.Deserialize(deserializer);
            }

            return instance;
        }
        
        
    }
}