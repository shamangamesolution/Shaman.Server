using System;

namespace Shaman.Game.Contract
{
    public interface IRoomContext
    {
        Guid GetRoomId();
        void KickPlayer(Guid sessionId);
        void Send(MessageData messageData, SendOptions sendOptions, params Guid[] sessionIds);
        void SendToAll(MessageData messageData, SendOptions sendOptions, params Guid[] exceptionSessionIds);
        void Open();
        void Close();
    }
}