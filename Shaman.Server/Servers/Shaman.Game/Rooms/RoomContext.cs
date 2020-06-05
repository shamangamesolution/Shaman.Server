using System;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;
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

        public int SendToAll(MessageBase message, params Guid[] exceptions)
        {
            return _room.SendToAll(message, exceptions);
        }

        public int AddToSendQueue(MessageBase message, Guid sessionId)
        {
            return _room.AddToSendQueue(message, sessionId);
        }

        public void KickPlayer(Guid sessionId)
        {
            _room.GetPlayer(sessionId).Peer.Disconnect(DisconnectReason.KickedByServer);
        }
        
        public void SendToAll(MessageData messageData, ushort opCode, bool isReliable, bool isOrdered,
            params Guid[] exceptions)
        {
            _room.SendToAll(messageData, opCode, isReliable, isOrdered, exceptions);
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