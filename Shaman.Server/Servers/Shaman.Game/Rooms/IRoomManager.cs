using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Messages;
using Shaman.Game.Contract;

namespace Shaman.Game.Rooms
{
    public interface IRoomManager
    {
        Guid CreateRoom(Dictionary<byte, object> properties, Dictionary<Guid, Dictionary<byte, object>> players,
            Guid? roomId);
        void UpdateRoom(Guid roomId, Dictionary<Guid, Dictionary<byte, object>> players);

        void DeleteRoom(Guid roomId);
        List<IRoom> GetAllRooms();
        int GetRoomsCount();
        int GetPlayersCount();
        IRoom GetRoomBySessionId(Guid sessionId);
        Task PeerJoined(IPeer peer, Guid roomId, Dictionary<byte, object> peerProperties);
        bool IsInRoom(Guid sessionId);
        void PeerLeft(Guid sessionId);
        void PeerDisconnected(Guid sessionId);
        void ProcessMessage(ushort operationCode, MessageData message, IPeer peer);
        Dictionary<Guid, int> GetRoomPeerCount();
        IRoom GetOldestRoom();
        void ConfirmedJoin(Guid sessionId, IRoom room);
    }
}