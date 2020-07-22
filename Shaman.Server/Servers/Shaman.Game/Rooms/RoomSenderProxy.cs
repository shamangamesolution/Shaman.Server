using System;
using Shaman.Common.Contract;
using Shaman.Contract.Bundle;

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

        public void Send(Payload payload, DeliveryOptions deliveryOptions, params Guid[] sessionIds)
        {
            _roomSender.Send(payload, deliveryOptions, sessionIds);
        }

        public void SendToAll(Payload payload, DeliveryOptions deliveryOptions, params Guid[] exceptionSessionIds)
        {
            _roomSender.SendToAll(payload, deliveryOptions, exceptionSessionIds);
        }
    }
}