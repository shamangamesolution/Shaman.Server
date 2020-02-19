using System;
using Shaman.Common.Utils.Messages;

namespace Shaman.Game.Contract
{
    public interface IRoomContext
    {
        Guid GetRoomId();
        void SendToAll(MessageBase message, params Guid[] exceptions);
        void AddToSendQueue(MessageBase message, Guid sessionId);
        void KickPlayer(Guid sessionId);
        void SendToAll(MessageData messageData, ushort opCode, bool isReliable, bool isOrdered,
            params Guid[] exceptions);
    }
}