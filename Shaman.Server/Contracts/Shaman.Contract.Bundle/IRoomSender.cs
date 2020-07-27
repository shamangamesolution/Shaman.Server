using System;
using Shaman.Common.Contract;

namespace Shaman.Contract.Bundle
{
    public interface IRoomSender
    {
        void Send(Payload payload, DeliveryOptions deliveryOptions, Guid sessionId);
        void Send(Payload payload, DeliveryOptions deliveryOptions, params Guid[] sessionIds);
        void SendToAll(Payload payload, DeliveryOptions deliveryOptions, Guid exceptionSessionId);
        void SendToAll(Payload payload, DeliveryOptions deliveryOptions, params Guid[] exceptionSessionIds);
    }
}