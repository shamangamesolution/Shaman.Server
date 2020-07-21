using System;
using Shaman.Common.Server.Peers;
using Shaman.Game.Contract;

namespace Shaman.Game.Rooms
{
    public class RoomContext : IRoomContext
    {
        private readonly IRoom _room;

        public RoomContext(IRoom room)
        {
            _room = room;
        }

        public Guid GetRoomId()
        {
            return _room.GetRoomId();
        }

        public void KickPlayer(Guid sessionId)
        {
            _room.GetPlayer(sessionId).Peer.Disconnect(ServerDisconnectReason.KickedByServer);
        }

        public void Send(MessageData messageData, SendOptions sendOptions, params Guid[] sessionIds)
        {
            _room.Send(messageData, sendOptions, sessionIds);
        }

        public void SendToAll(MessageData messageData, SendOptions sendOptions, params Guid[] exceptionSessionIds)
        {
            _room.SendToAll(messageData, sendOptions, exceptionSessionIds);
        }


        public void Open()
        {
            _room.Open();
        }

        public void Close()
        {
            _room.Close();
        }
    }
}