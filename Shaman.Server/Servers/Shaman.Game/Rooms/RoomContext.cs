using System;
using Shaman.Common.Contract;
using Shaman.Common.Server.Peers;
using Shaman.Contract.Bundle;

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

        public void Send(Payload payload, DeliveryOptions transportOptions, params Guid[] sessionIds)
        {
            _room.Send(payload, transportOptions, sessionIds);
        }

        public void SendToAll(Payload payload, DeliveryOptions transportOptions, params Guid[] exceptionSessionIds)
        {
            _room.SendToAll(payload, transportOptions, exceptionSessionIds);
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