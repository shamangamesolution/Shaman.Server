using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Authorization;
using Shaman.Messages.General;
using Shaman.Messages.General.DTO.Events;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;
using Shaman.Messages.MM;
using Shaman.Messages.RoomFlow;


namespace Shaman.Messages
{
    public class MessageFactory
    {
       
        public static MessageBase DeserializeMessage(ushort operationCode, ISerializerFactory serializerFactory, byte[] byteArray)
        {
            var messageType = MessageBase.GetMessageType(byteArray);
            
            switch (operationCode)
            {
                
                case CustomOperationCode.Ping:
                    return MessageBase.DeserializeAs<PingEvent>(serializerFactory, byteArray);
                case CustomOperationCode.Connect:
                    return MessageBase.DeserializeAs<ConnectedEvent>(serializerFactory, byteArray);
                case CustomOperationCode.Test:
                    return MessageBase.DeserializeAs<TestRoomEvent>(serializerFactory, byteArray);
                case CustomOperationCode.LeaveRoom:
                    return MessageBase.DeserializeAs<LeaveRoomEvent>(serializerFactory, byteArray);
                case CustomOperationCode.Disconnect:
                    return MessageBase.DeserializeAs<DisconnectEvent>(serializerFactory, byteArray);        
                case CustomOperationCode.JoinRoom when messageType == MessageType.Request:
                    return MessageBase.DeserializeAs<JoinRoomRequest>(serializerFactory, byteArray);
                case CustomOperationCode.JoinRoom when messageType == MessageType.Response:
                    return MessageBase.DeserializeAs<JoinRoomResponse>(serializerFactory, byteArray);
                case CustomOperationCode.EnterMatchMaking:
                    return MessageBase.DeserializeAs<EnterMatchMakingResponse>(serializerFactory, byteArray);
                case CustomOperationCode.JoinInfo:
                    return MessageBase.DeserializeAs<JoinInfoEvent>(serializerFactory, byteArray);
                case CustomOperationCode.LeaveMatchMaking:
                    return MessageBase.DeserializeAs<LeaveMatchMakingResponse>(serializerFactory, byteArray);
                case CustomOperationCode.Authorization:
                    return MessageBase.DeserializeAs<AuthorizationResponse>(serializerFactory, byteArray);
                case CustomOperationCode.PingRequest when messageType == MessageType.Request:
                    return MessageBase.DeserializeAs<PingRequest>(serializerFactory, byteArray);
                case CustomOperationCode.PingRequest when messageType == MessageType.Response:
                    return MessageBase.DeserializeAs<PingResponse>(serializerFactory, byteArray);
                default:
                    throw new ArgumentNullException($"MessageFactory.GetMessage error: Operation code {operationCode} is not supported");

            }
        }
        
        
    }
}