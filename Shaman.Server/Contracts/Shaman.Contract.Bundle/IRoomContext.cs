using System;

namespace Shaman.Contract.Bundle
{
    public interface IRoomContext
    {
        Guid GetRoomId();
        void KickPlayer(Guid sessionId);
        IRoomSender GetSender();
        void Open();
        void Close();
    }
}