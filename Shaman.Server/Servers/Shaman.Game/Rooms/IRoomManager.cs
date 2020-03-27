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

        bool CanJoinRoom(Guid roomId);
        List<IRoom> GetAllRooms();
        int GetRoomsCount();
        IRoom GetRoomBySessionId(Guid sessionId);
        bool IsInRoom(Guid sessionId);
        void ProcessMessage(ushort operationCode, MessageData message, IPeer peer);
        Dictionary<Guid, int> GetRoomPeerCount();
        IRoom GetOldestRoom();
        void ConfirmedJoin(Guid sessionId, IRoom room);
        void PeerDisconnected(IPeer peer);
    }
}