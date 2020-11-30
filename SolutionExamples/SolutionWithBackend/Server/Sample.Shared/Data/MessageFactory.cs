using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sample.Shared.Data.DTO.Requests;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;

namespace Sample.Shared.Data
{
    public class MessageFactory
    {
        private static readonly Lazy<Dictionary<ushort, Type>> TypesMap = new Lazy<Dictionary<ushort, Type>>(CreateMessageMap);
        static Dictionary<ushort, Type> CreateMessageMap()
        {
            var map1 = CreateMessageMap1();
            var map2 = CreateMessageMap2();
            foreach (var entry in map2)
            {
                map1.Add(entry.Key, entry.Value);
            }

            return map1;
        }
        static Dictionary<ushort, Type> CreateMessageMap1()
        {
            var messageBaseType = typeof(InitializationRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) != null)
                .ToArray();
            return messageTypes.Select(Activator.CreateInstance).OfType<MessageBase>()
                .ToDictionary(k => k.OperationCode, v => v.GetType());
        }
        static Dictionary<ushort, Type> CreateMessageMap2()
        {
            var messageBaseType = typeof(PingRequest);
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) != null)
                .ToArray();
            return messageTypes
                .Select(Activator.CreateInstance)
                .OfType<MessageBase>()
                .ToDictionary(k => k.OperationCode, v => v.GetType());
        }
        public static MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer, byte[] byteArray)
        {
            return DeserializeMessage(operationCode, serializer, byteArray, 0, byteArray.Length);
        }

        public static MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer,
            byte[] byteArray, int offset, int length)
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