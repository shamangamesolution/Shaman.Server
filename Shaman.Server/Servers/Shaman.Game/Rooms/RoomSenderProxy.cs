using System;
using System.Collections.Generic;
using Shaman.Contract.Bundle;
using Shaman.Contract.Common;

namespace Shaman.Game.Rooms
{
    /// <summary>
    /// To avoid abuse real implementation in bundle
    /// </summary>
    public class RoomSenderProxy : IRoomSender
    {
        private readonly IRoomSender _roomSender;

        public RoomSenderProxy(IRoomSender roomSender)
        {
            _roomSender = roomSender;
        }

        public void Send(Payload payload, DeliveryOptions deliveryOptions, Guid sessionId)
        {
            _roomSender.Send(payload, deliveryOptions, sessionId);
        }

        public void Send(Payload payload, DeliveryOptions deliveryOptions, IEnumerable<Guid> sessionIds)
        {
            _roomSender.Send(payload, deliveryOptions, sessionIds);
        }

        public void SendToAll(Payload payload, DeliveryOptions deliveryOptions, Guid exceptionSessionId)
        {
            _roomSender.SendToAll(payload, deliveryOptions, exceptionSessionId);
        }

        public void SendToAll(Payload payload, DeliveryOptions deliveryOptions)
        {
            _roomSender.SendToAll(payload, deliveryOptions);
        }
    }
}