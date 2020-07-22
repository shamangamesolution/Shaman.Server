using System;
using Shaman.Common.Contract;

namespace Shaman.Contract.Bundle
{
    public interface IRoomContext
    {
        Guid GetRoomId();
        void KickPlayer(Guid sessionId);
        void Send(Payload payload, DeliveryOptions transportOptions, params Guid[] sessionIds);
        void SendToAll(Payload payload, DeliveryOptions transportOptions, params Guid[] exceptionSessionIds);
        void Open();
        void Close();
    }
}