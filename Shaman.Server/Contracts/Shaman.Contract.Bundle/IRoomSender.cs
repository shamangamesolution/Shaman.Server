using System;
using System.Collections.Generic;
using Shaman.Contract.Common;

namespace Shaman.Contract.Bundle
{
    public interface IRoomSender
    {
        void Send(Payload payload, DeliveryOptions deliveryOptions, Guid sessionId);
        void Send(Payload payload, DeliveryOptions deliveryOptions, IEnumerable<Guid> sessionIds);
        void SendToAll(Payload payload, DeliveryOptions deliveryOptions, Guid exceptionSessionId);
        void SendToAll(Payload payload, DeliveryOptions deliveryOptions);
    }
}