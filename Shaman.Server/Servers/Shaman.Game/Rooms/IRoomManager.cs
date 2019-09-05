using System;
using System.Collections.Generic;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;

namespace Shaman.Game.Rooms
{
    public interface IRoomManager
    {
        Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players);
        void DeleteRoom(Guid roomId);
        List<IRoom> GetAllRooms();
        int GetRoomsCount();
        int GetPlayersCount();
        IRoom GetRoomBySessionId(Guid sessionId);
        void PeerJoined(IPeer peer, Guid roomId, Dictionary<byte, object> peerProperties);
        bool IsInRoom(Guid sessionId);
        void PeerLeft(Guid sessionId);
        void PeerDisconnected(Guid sessionId);
        void ProcessMessage(MessageBase message, IPeer peer);
        Dictionary<Guid, int> GetRoomPeerCount();
        IRoom GetOldestRoom();
        void ConfirmedJoin(Guid sessionId, IRoom room);
    }
}