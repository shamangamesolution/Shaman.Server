using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests;

namespace Shaman.TestTools.ClientPeers
{
    public static class MessageFactory
    {
        private static readonly Lazy<Dictionary<int, Type>> TypesMap = new Lazy<Dictionary<int, Type>>(CreateMessageMap);

        static Dictionary<int, Type> CreateMessageMap()
        {
            var messageBaseType = typeof(PingRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) != null)
                .ToArray();
            return messageTypes.Select(Activator.CreateInstance).OfType<MessageBase>()
                .ToDictionary(k => k.OperationCode << 8 | (int) k.Type, v => v.GetType());
        }

        public static MessageBase DeserializeMessageForTest(ushort operationCode, ISerializer serializer,
            byte[] byteArray, int offset, int length)
        {
            var messageType = MessageBase.GetMessageType(byteArray, offset);

            var key = operationCode << 8 | (int) messageType;
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