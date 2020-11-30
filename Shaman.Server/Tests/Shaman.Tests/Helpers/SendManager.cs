using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;
using Shaman.Serialization;
using Shaman.Serialization.Messages.Udp;
using Shaman.Serialization.Room;

namespace Shaman.Tests.Helpers
{
    public interface ISendManager
    {
        int Send(MessageBase message, Guid sessionId);
        int SendToAll(MessageBase message);
        int SendToAll(MessageBase message, Guid exceptionSessionId);
    }
    
    public class SendManager : ISendManager
    {
        private ShamanRoomSender _sender;
        
        public SendManager(IRoomContext roomContext, ISerializer serializer)
        {
            _sender = new ShamanRoomSender(roomContext.GetSender(), serializer);
        }
        
        public int Send(MessageBase message, Guid sessionId)
        {
            return _sender.Send(message, new DeliveryOptions(message.IsReliable, message.IsOrdered), sessionId);
        }

        public int SendToAll(MessageBase message)
        {
            return _sender.SendToAll(message, new DeliveryOptions(message.IsReliable, message.IsOrdered));
        }

        public int SendToAll(MessageBase message, Guid exceptionSessionId)
        {
            return _sender.SendToAll(message, new DeliveryOptions(message.IsReliable, message.IsOrdered), exceptionSessionId);
        }
    }
}