using System;
using Shaman.Common.Server.Peers;
using Shaman.Contract.Bundle;

namespace Shaman.Game.Rooms
{
    public class RoomContext : IRoomContext
    {
        private readonly IRoom _room;
        private readonly RoomSenderProxy _roomSender;

        public RoomContext(IRoom room)
        {
            _room = room;
            _roomSender = new RoomSenderProxy(room);
        }

        public Guid GetRoomId()
        {
            return _room.GetRoomId();
        }

        public void KickPlayer(Guid sessionId)
        {
            if (_room.TryGetPlayer(sessionId, out var player))
                player.Peer.Disconnect(ServerDisconnectReason.KickedByServer);
        }

        public IRoomSender GetSender()
        {
            return _roomSender;
        }

        public void Open()
        {
            _room.Open();
        }

        public void Close()
        {
            _room.Close();
        }
        public void Dispose()
        {
            _room.InvalidateRoom();
        }
    }
}