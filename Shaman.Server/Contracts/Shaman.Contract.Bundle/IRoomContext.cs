using System;
using Shaman.Common.Contract;

namespace Shaman.Contract.Bundle
{
    public interface IRoomContext
    {
        Guid GetRoomId();
        void KickPlayer(Guid sessionId);
        void Send(MessageData messageData, DeliveryOptions transportOptions, params Guid[] sessionIds);
        void SendToAll(MessageData messageData, DeliveryOptions transportOptions, params Guid[] exceptionSessionIds);
        void Open();
        void Close();
    }
}