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
        private static readonly Lazy<Dictionary<int, Type>> TypesMap = new Lazy<Dictionary<int, Type>>(CreateMessageMap);

        static Dictionary<int, Type> CreateMessageMap()
        {
            var messageBaseType = typeof(PingRequest);
            var messageBundleType = typeof(GetStorageHttpRequest);
            
            var messageTypes = messageBaseType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) != null).ToList();
                //.ToArray();
            var messageBundleTypes = messageBundleType.Assembly.GetTypes().Where(t =>
                    t.IsSubclassOf(typeof(MessageBase)) && !t.IsAbstract &&
                    t.GetConstructor(Array.Empty<Type>()) != null).ToList();
            
            messageTypes.AddRange(messageBundleTypes);

            var instances = messageTypes.Select(Activator.CreateInstance).OfType<MessageBase>();

            var result = new Dictionary<int, Type>();
            foreach(var item in instances)
                if (!result.ContainsKey(item.OperationCode << 8 | (int) item.Type))
                    result.Add(item.OperationCode << 8 | (int) item.Type, item.GetType());
            
            return result;
        }

        public static MessageBase DeserializeMessage(ushort operationCode, ISerializer serializer,
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
        
        
        public static MessageBase DeserializeMessage(ushort operationCode, ISerializer serializerFactory, byte[] byteArray)
        {
            var messageType = MessageBase.GetMessageType(byteArray);
            
            switch (operationCode)
            {
                case CustomOperationCode.Ping:
                    return serializerFactory.DeserializeAs<PingEvent>(byteArray);
                case CustomOperationCode.Connect:
                    return serializerFactory.DeserializeAs<ConnectedEvent>(byteArray);
                case CustomOperationCode.Test:
                    return serializerFactory.DeserializeAs<TestRoomEvent>(byteArray);
                case CustomOperationCode.LeaveRoom:
                    return serializerFactory.DeserializeAs<LeaveRoomEvent>(byteArray);
                case CustomOperationCode.Disconnect:
                    return serializerFactory.DeserializeAs<DisconnectEvent>(byteArray);        
                case CustomOperationCode.JoinRoom when messageType == MessageType.Request:
                    return serializerFactory.DeserializeAs<JoinRoomRequest>(byteArray);
                case CustomOperationCode.JoinRoom when messageType == MessageType.Response:
                    return serializerFactory.DeserializeAs<JoinRoomResponse>(byteArray);
                case CustomOperationCode.EnterMatchMaking:
                    return serializerFactory.DeserializeAs<EnterMatchMakingResponse>(byteArray);
                case CustomOperationCode.JoinInfo:
                    return serializerFactory.DeserializeAs<JoinInfoEvent>(byteArray);
                case CustomOperationCode.LeaveMatchMaking:
                    return serializerFactory.DeserializeAs<LeaveMatchMakingResponse>(byteArray);
                case CustomOperationCode.Authorization:
                    return serializerFactory.DeserializeAs<AuthorizationResponse>(byteArray);
                case CustomOperationCode.PingRequest when messageType == MessageType.Request:
                    return serializerFactory.DeserializeAs<PingRequest>(byteArray);
                case CustomOperationCode.PingRequest when messageType == MessageType.Response:
                    return serializerFactory.DeserializeAs<PingResponse>(byteArray);
                default:
                    throw new ArgumentNullException($"MessageFactory.GetMessage error: Operation code {operationCode} is not supported");

            }
        }
        
        
    }
}